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
            string? filterExpression,
            Dictionary<string, AttributeValue> values,
            int limit,
            string? paginationToken,
            bool scanIndexForward = false)
        {
            var expressionValues = new Dictionary<string, AttributeValue>(values)
            {
                [":userId"] = new AttributeValue { S = userId }
            };

            var request = new QueryRequest
            {
                TableName = tableName,
                KeyConditionExpression = "UserId = :userId",
                ExpressionAttributeValues = expressionValues,
                ScanIndexForward = scanIndexForward
            };

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
