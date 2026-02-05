using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System.Text;

namespace MyApp.Data.Dynamo
{
    // DynamoDB query helper class
    public static class DynamoQueryHelper
    {
        // Create a paginated query request for a specific user
        public static QueryRequest CreateUserIdQuery(
            string tableName,
            string userId,
            // can be "UserId = :userId" with optional filters
            string keyConditionExpression,
            string? filterExpression,
            Dictionary<string, AttributeValue> values,
            int limit,
            string? paginationToken,
            // scanIndexForward controls the sort order of query results. For DynamoDB Query:
            // - true (default): ascending order by the sort key.
            // - false: descending order by the sort key.
            bool scanIndexForward = false,
            // tells DynamoDB to run the query against a specific index instead of the base table. If you omit it, the query runs against the full table’s primary key.
            string? indexName = null)
        {
            var expressionValues = new Dictionary<string, AttributeValue>(values)
            {
                [":userId"] = new AttributeValue { S = userId }
            };

            var request = new QueryRequest
            {
                TableName = tableName,
                KeyConditionExpression = keyConditionExpression,
                ExpressionAttributeValues = expressionValues,
                ScanIndexForward = scanIndexForward
            };

            if (!string.IsNullOrWhiteSpace(indexName))
            {
                request.IndexName = indexName;
            }

            if (limit > 0)
            {
                request.Limit = limit;
            }

            if (!string.IsNullOrWhiteSpace(filterExpression))
            {
                request.FilterExpression = filterExpression;
            }

            Dictionary<string, AttributeValue>? exclusiveStartKey = DecodePaginationToken(paginationToken);
            if (exclusiveStartKey != null && exclusiveStartKey.Count > 0)
            {
                request.ExclusiveStartKey = exclusiveStartKey;
            }

            return request;
        }

        public static string? EncodePaginationToken(Dictionary<string, AttributeValue>? lastEvaluatedKey)
        {
            if (lastEvaluatedKey == null || lastEvaluatedKey.Count == 0)
            {
                return null;
            }

            Document document = Document.FromAttributeMap(lastEvaluatedKey);
            string json = document.ToJson();
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        }

        public static Dictionary<string, AttributeValue>? DecodePaginationToken(string? paginationToken)
        {
            if (string.IsNullOrWhiteSpace(paginationToken))
            {
                return null;
            }

            string json = Encoding.UTF8.GetString(Convert.FromBase64String(paginationToken));
            Document document = Document.FromJson(json);
            return document.ToAttributeMap();
        }
    }
}
