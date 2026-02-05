using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using System;
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
            if (status == TodoStatus.Deleted)
            {
                return (Array.Empty<TodoItem>(), null);
            }

            string keyConditionExpression = "UserId = :userId";
            string? filterExpression = null;
            var values = new Dictionary<string, AttributeValue>();

            if (status.HasValue)
            {
                filterExpression = "StatusCode = :statusCode";
                values[":statusCode"] = new AttributeValue { N = ((int)status.Value).ToString() };
            }

            return await QueryPageAsync(
                userId,
                keyConditionExpression,
                filterExpression,
                values,
                limit,
                paginationToken,
                indexName: "UserIdActiveTodoId");
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

            string keyConditionExpression = "UserId = :userId";

            // For search, omit the DynamoDB limit to use the default 1 MB page size.
            return await QueryPageAsync(
                userId,
                keyConditionExpression,
                filterExpression,
                values,
                0,
                paginationToken,
                indexName: "UserIdActiveTodoId");
        }

        // Query DynamoDB for todo items with optional filter
        // ref: https://codewithmukesh.com/blog/pagination-in-amazon-dynamodb-with-dotnet/
        private async Task<(IEnumerable<TodoItem> Items, string? NextToken)> QueryPageAsync(
            string userId,
            string keyConditionExpression,
            string? filterExpression,
            Dictionary<string, AttributeValue> values,
            int limit,
            string? paginationToken,
            string? indexName = null)
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
                indexName: indexName);

            QueryResponse response = await _client.QueryAsync(request);
            List<TodoItem> items = response.Items
                .Select(item => _context.FromDocument<TodoItem>(Document.FromAttributeMap(item)))
                .ToList();

            string? nextToken = DynamoQueryHelper.EncodePaginationToken(response.LastEvaluatedKey);
            return (items, nextToken);
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
            todo.ActiveTodoId = null;
            await _context.SaveAsync(todo);
            return true;
        }
    }
}
