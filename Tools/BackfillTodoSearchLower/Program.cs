using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace BackfillTodoSearchLower;

// script to backfill TitleLower/ContentLower in Todos table
internal static class Program
{
    private const string DefaultTableName = "Todos";

    private static readonly Dictionary<string, string> ProjectionAttributeNames = new()
    {
        ["#userId"] = "UserId",
        ["#todoId"] = "TodoId",
        ["#title"] = "Title",
        ["#content"] = "Content",
        ["#titleLower"] = "TitleLower",
        ["#contentLower"] = "ContentLower"
    };

    private static async Task<int> Main(string[] args)
    {
        string tableName = GetArgValue(args, "--table", "-t") ?? DefaultTableName;
        string? region = GetArgValue(args, "--region", "-r");
        bool dryRun = args.Any(arg => string.Equals(arg, "--dry-run", StringComparison.OrdinalIgnoreCase));
        int? pageSize = TryParseInt(GetArgValue(args, "--page-size", "-p"));

        Console.WriteLine($"Backfill TitleLower/ContentLower in table '{tableName}'.");
        Console.WriteLine(dryRun ? "Mode: dry-run" : "Mode: write");
        Console.WriteLine(pageSize.HasValue ? $"Page size: {pageSize}" : "Page size: default");

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
            var request = new ScanRequest
            {
                TableName = tableName,
                ProjectionExpression = "#userId, #todoId, #title, #content, #titleLower, #contentLower",
                ExpressionAttributeNames = new Dictionary<string, string>(ProjectionAttributeNames),
                ExclusiveStartKey = lastEvaluatedKey
            };

            if (pageSize.HasValue && pageSize.Value > 0)
            {
                request.Limit = pageSize.Value;
            }

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

                if (!TryGetString(item, "Title", out var title))
                {
                    continue;
                }

                string desiredTitleLower = title.ToLowerInvariant();
                string? desiredContentLower = TryGetString(item, "Content", out var content)
                    ? content.ToLowerInvariant()
                    : null;

                bool hasTitleLower = TryGetString(item, "TitleLower", out var titleLower);
                bool hasContentLower = TryGetString(item, "ContentLower", out var contentLower);

                var setActions = new List<string>();
                var removeActions = new List<string>();
                var expressionAttributeValues = new Dictionary<string, AttributeValue>();

                if (!hasTitleLower || !string.Equals(titleLower, desiredTitleLower, StringComparison.Ordinal))
                {
                    setActions.Add("#titleLower = :titleLower");
                    expressionAttributeValues[":titleLower"] = new AttributeValue { S = desiredTitleLower };
                }

                if (!string.IsNullOrWhiteSpace(desiredContentLower))
                {
                    if (!hasContentLower || !string.Equals(contentLower, desiredContentLower, StringComparison.Ordinal))
                    {
                        setActions.Add("#contentLower = :contentLower");
                        expressionAttributeValues[":contentLower"] = new AttributeValue { S = desiredContentLower };
                    }
                }
                else if (hasContentLower)
                {
                    removeActions.Add("#contentLower");
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

                    bool usesTitleLower = setActions.Any(action => action.Contains("#titleLower", StringComparison.Ordinal))
                        || removeActions.Any(action => action.Contains("#titleLower", StringComparison.Ordinal));
                    bool usesContentLower = setActions.Any(action => action.Contains("#contentLower", StringComparison.Ordinal))
                        || removeActions.Any(action => action.Contains("#contentLower", StringComparison.Ordinal));

                    var expressionAttributeNames = new Dictionary<string, string>();
                    if (usesTitleLower)
                    {
                        expressionAttributeNames["#titleLower"] = "TitleLower";
                    }

                    if (usesContentLower)
                    {
                        expressionAttributeNames["#contentLower"] = "ContentLower";
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

                    if (expressionAttributeNames.Count > 0)
                    {
                        updateRequest.ExpressionAttributeNames = expressionAttributeNames;
                    }

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

    private static int? TryParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (int.TryParse(value, out int result))
        {
            return result;
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
}
