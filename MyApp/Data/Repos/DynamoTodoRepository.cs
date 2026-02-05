using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System.Linq;
using MyApp.Data.Dynamo;
using MyApp.Models.Entity;
using MyApp.Models.Enum;

namespace MyApp.Data.Repos
{
    public class DynamoTodoRepository : ITodoRepository
    {
        private const string TableName = "Todos";
        private readonly IAmazonDynamoDB _client;
        private readonly IDynamoDBContext _context;

        public DynamoTodoRepository(IAmazonDynamoDB client, IDynamoDBContext context)
        {
            _client = client;
            _context = context;
        }

        public async Task<TodoItem> GetTodoAsync(string userId, string todoId)
        {
            return await _context.LoadAsync<TodoItem>(userId, todoId);
        }

        public async Task<(IEnumerable<TodoItem> Items, string? NextToken)> QueryTodosPageAsync(
            string userId,
            TodoStatus? status,
            int limit,
            string? paginationToken)
        {
            // return a key condition expression and bound values
            var (keyConditionExpression, values) = BuildStatusKeyCondition(status);
            // no need to pass filterExpression as status is part of the key
            return await QueryPageAsync(userId, keyConditionExpression, null, values, limit, paginationToken);
        }

        public async Task<(IEnumerable<TodoItem> Items, string? NextToken)> SearchTodosPageAsync(
            string userId,
            string? query,
            int limit,
            string? paginationToken)
        {
            // if query is empty, return all todos
            if (string.IsNullOrWhiteSpace(query))
            {
                return await QueryTodosPageAsync(userId, null, limit, paginationToken);
            }

            var values = new Dictionary<string, AttributeValue>
            {
                [":query"] = new AttributeValue { S = query.Trim() }
            };

            const string filterExpression =
                "contains(Title, :query) OR contains(Content, :query)";

            var (keyConditionExpression, keyValues) = BuildStatusKeyCondition(null);
            foreach (var kvp in values)
            {
                keyValues[kvp.Key] = kvp.Value;
            }

            return await QueryPageAsync(userId, keyConditionExpression, filterExpression, keyValues, limit, paginationToken);
        }

        // Query DynamoDB for todo items with optional filter
        // ref: https://codewithmukesh.com/blog/pagination-in-amazon-dynamodb-with-dotnet/
        private async Task<(IEnumerable<TodoItem> Items, string? NextToken)> QueryPageAsync(
            string userId,
            // keyConditionExpression can be "UserId = :userId" or "UserId = :userId AND StatusTodoId BETWEEN :statusStart AND :statusEnd"
            string keyConditionExpression,
            string? filterExpression,
            Dictionary<string, AttributeValue> values,
            int limit,
            string? paginationToken)
        {
            QueryRequest request = DynamoQueryHelper.CreateUserIdQuery(
                TableName,
                userId,
                keyConditionExpression,
                filterExpression,
                values,
                limit,
                paginationToken,
                scanIndexForward: false,
                indexName: "UserIdStatusTodoId");

            QueryResponse response = await _client.QueryAsync(request);
            List<TodoItem> items = response.Items
                .Select(item => _context.FromDocument<TodoItem>(Document.FromAttributeMap(item)))
                .ToList();

            string? nextToken = DynamoQueryHelper.EncodePaginationToken(response.LastEvaluatedKey);
            return (items, nextToken);
        }

        // Build key condition expression and values for status
        internal static (string KeyConditionExpression, Dictionary<string, AttributeValue> Values) BuildStatusKeyCondition(
            TodoStatus? status)
        {
            if (status.HasValue)
            {
                // convert enum to string with at least 1 digit
                string statusPrefix = ((int)status.Value).ToString("D1") + "#";
                // return expression with given status
                return (
                    "UserId = :userId AND StatusTodoId BETWEEN :statusStart AND :statusEnd",
                    new Dictionary<string, AttributeValue>
                    {
                        [":statusStart"] = new AttributeValue { S = statusPrefix },
                        [":statusEnd"] = new AttributeValue { S = statusPrefix + "~" }
                    }
                );
            }

            // return expression with all non-deleted status
            return (
                "UserId = :userId AND StatusTodoId BETWEEN :statusStart AND :statusEnd",
                new Dictionary<string, AttributeValue>
                {
                    [":statusStart"] = new AttributeValue { S = ((int)TodoStatus.Incomplete).ToString("D1") + "#" },
                    [":statusEnd"] = new AttributeValue { S = ((int)TodoStatus.Complete).ToString("D1") + "#~" }
                }
            );
        }

        public async Task<TodoItem> AddTodoAsync(TodoItem todo)
        {
            await _context.SaveAsync(todo);
            return todo;
        }

        public async Task<bool> DeleteTodoAsync(string userId, string todoId)
        {
            // Check if the item exists
            TodoItem existing = await _context.LoadAsync<TodoItem>(userId, todoId);
            if (existing == null)
            {
                return false; // nothing to delete
            }

            // Perform the deletion
            await _context.DeleteAsync<TodoItem>(existing);
            return true;
        }

        public async Task UpdateTodoAsync(TodoItem todo)
        {
            await _context.SaveAsync(todo);
        }

        public async Task<bool> MarkAsDeletedAsync(string userId, string todoId)
        {
            TodoItem todo = await _context.LoadAsync<TodoItem>(userId, todoId);

            if (todo == null) return false;

            todo.StatusCode = TodoStatus.Deleted; // 3
            // Update StatusTodoId for GSI query; D1 is for single digit
            todo.StatusTodoId = $"{(int)todo.StatusCode:D1}#{todo.TodoId}";
            await _context.SaveAsync(todo);
            return true;
        }
    }
}
