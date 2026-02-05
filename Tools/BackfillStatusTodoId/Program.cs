using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace BackfillActiveTodoId;

// script to backfill ActiveTodoId in Todos table
internal static class Program
{
    private const string DefaultTableName = "Todos";
    private const int DeletedStatusCode = 3;

    private static async Task<int> Main(string[] args)
    {
        string tableName = GetArgValue(args, "--table", "-t") ?? DefaultTableName;
        string? region = GetArgValue(args, "--region", "-r");
        bool dryRun = args.Any(arg => string.Equals(arg, "--dry-run", StringComparison.OrdinalIgnoreCase));
        // cleanupStatusTodoId is only for cleaning up StatusTodoId, not for backfilling ActiveTodoId
        bool cleanupStatusTodoId = args.Any(arg => string.Equals(arg, "--cleanup-status-todo-id", StringComparison.OrdinalIgnoreCase));

        Console.WriteLine($"Backfill ActiveTodoId in table '{tableName}'.");
        Console.WriteLine(dryRun ? "Mode: dry-run" : "Mode: write");
        Console.WriteLine(cleanupStatusTodoId ? "Cleanup: StatusTodoId" : "Cleanup: none");

        using var client = string.IsNullOrWhiteSpace(region)
            ? new AmazonDynamoDBClient()
            : new AmazonDynamoDBClient(new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
            });

        int scanned = 0;
        int updated = 0;
        Dictionary<string, AttributeValue>? lastEvaluatedKey = null;

        do
        {
            // clean up status todo id if cleanupStatusTodoId is true
            string projectionExpression = cleanupStatusTodoId
                ? "UserId, TodoId, StatusCode, ActiveTodoId, StatusTodoId"
                : "UserId, TodoId, StatusCode, ActiveTodoId";

            var request = new ScanRequest
            {
                TableName = tableName,
                ProjectionExpression = projectionExpression,
                ExclusiveStartKey = lastEvaluatedKey
            };

            var response = await client.ScanAsync(request);
            foreach (var item in response.Items)
            {
                scanned++;
                if (!TryGetString(item, "UserId", out var userId))
                {
                    continue;
                }

                if (!TryGetString(item, "TodoId", out var todoId))
                {
                    continue;
                }

                if (!TryGetNumber(item, "StatusCode", out var statusCode))
                {
                    continue;
                }

                bool isDeleted = statusCode == DeletedStatusCode;
                bool hasActiveTodoId = TryGetString(item, "ActiveTodoId", out var activeTodoId);
                bool hasStatusTodoId = cleanupStatusTodoId && TryGetString(item, "StatusTodoId", out _);

                var setActions = new List<string>();
                var removeActions = new List<string>();
                var expressionAttributeValues = new Dictionary<string, AttributeValue>();

                if (isDeleted)
                {
                    if (hasActiveTodoId)
                    {
                        removeActions.Add("ActiveTodoId");
                    }
                }
                else if (!hasActiveTodoId || !string.Equals(activeTodoId, todoId, StringComparison.Ordinal))
                {
                    setActions.Add("ActiveTodoId = :activeTodoId");
                    expressionAttributeValues[":activeTodoId"] = new AttributeValue { S = todoId };
                }

                if (cleanupStatusTodoId && hasStatusTodoId)
                {
                    removeActions.Add("StatusTodoId");
                }

                if (setActions.Count == 0 && removeActions.Count == 0)
                {
                    continue;
                }

                if (!dryRun)
                {
                    string updateExpression = string.Empty;
                    if (setActions.Count > 0)
                    {
                        updateExpression = "SET " + string.Join(", ", setActions);
                    }

                    if (removeActions.Count > 0)
                    {
                        updateExpression += (updateExpression.Length > 0 ? " " : string.Empty)
                            + "REMOVE " + string.Join(", ", removeActions);
                    }

                    var updateRequest = new UpdateItemRequest
                    {
                        TableName = tableName,
                        Key = new Dictionary<string, AttributeValue>
                        {
                            ["UserId"] = new AttributeValue { S = userId },
                            ["TodoId"] = new AttributeValue { S = todoId }
                        },
                        UpdateExpression = updateExpression
                    };

                    if (expressionAttributeValues.Count > 0)
                    {
                        updateRequest.ExpressionAttributeValues = expressionAttributeValues;
                    }

                    await client.UpdateItemAsync(updateRequest);
                }

                updated++;
            }

            lastEvaluatedKey = response.LastEvaluatedKey;
        }
        while (lastEvaluatedKey != null && lastEvaluatedKey.Count > 0);

        Console.WriteLine($"Scanned: {scanned}");
        Console.WriteLine($"Updated: {updated}");

        return 0;
    }

    private static string? GetArgValue(string[] args, string longName, string shortName)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], longName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(args[i], shortName, StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length)
                {
                    return args[i + 1];
                }
            }
        }

        return null;
    }

    private static bool TryGetString(
        Dictionary<string, AttributeValue> item,
        string key,
        out string value)
    {
        if (item.TryGetValue(key, out var attribute) && !string.IsNullOrWhiteSpace(attribute.S))
        {
            value = attribute.S;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static bool TryGetNumber(
        Dictionary<string, AttributeValue> item,
        string key,
        out int value)
    {
        if (item.TryGetValue(key, out var attribute) &&
            !string.IsNullOrWhiteSpace(attribute.N) &&
            int.TryParse(attribute.N, out value))
        {
            return true;
        }

        value = 0;
        return false;
    }
}
