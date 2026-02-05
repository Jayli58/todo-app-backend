using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace BackfillStatusTodoId;

// script to backfill StatusTodoId in Todos table
internal static class Program
{
    private const string DefaultTableName = "Todos";

    private static async Task<int> Main(string[] args)
    {
        string tableName = GetArgValue(args, "--table", "-t") ?? DefaultTableName;
        string? region = GetArgValue(args, "--region", "-r");
        bool dryRun = args.Any(arg => string.Equals(arg, "--dry-run", StringComparison.OrdinalIgnoreCase));

        Console.WriteLine($"Backfill StatusTodoId in table '{tableName}'.");
        Console.WriteLine(dryRun ? "Mode: dry-run" : "Mode: write");

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
                ProjectionExpression = "UserId, TodoId, StatusCode, StatusTodoId",
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

                string desired = $"{statusCode:D1}#{todoId}";
                string? current = null;
                if (TryGetString(item, "StatusTodoId", out var statusTodoId))
                {
                    current = statusTodoId;
                }

                if (string.Equals(current, desired, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!dryRun)
                {
                    var updateRequest = new UpdateItemRequest
                    {
                        TableName = tableName,
                        Key = new Dictionary<string, AttributeValue>
                        {
                            ["UserId"] = new AttributeValue { S = userId },
                            ["TodoId"] = new AttributeValue { S = todoId }
                        },
                        UpdateExpression = "SET StatusTodoId = :statusTodoId",
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                        {
                            [":statusTodoId"] = new AttributeValue { S = desired }
                        }
                    };

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
