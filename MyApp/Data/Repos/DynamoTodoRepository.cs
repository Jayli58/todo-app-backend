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
            // return a filter expression and bound values
            var (filterExpression, values) = BuildStatusFilter(status);
            return await QueryPageAsync(userId, filterExpression, values, limit, paginationToken);
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
                [":query"] = new AttributeValue { S = query.Trim() },
                [":incomplete"] = new AttributeValue { N = ((int)TodoStatus.Incomplete).ToString() },
                [":complete"] = new AttributeValue { N = ((int)TodoStatus.Complete).ToString() }
            };

            const string filterExpression =
                "(contains(Title, :query) OR contains(Content, :query)) AND " +
                "(StatusCode = :incomplete OR StatusCode = :complete)";

            return await QueryPageAsync(userId, filterExpression, values, limit, paginationToken);
        }

        // Query DynamoDB for todo items with optional filter
        private async Task<(IEnumerable<TodoItem> Items, string? NextToken)> QueryPageAsync(
            string userId,
            string filterExpression,
            Dictionary<string, AttributeValue> values,
            int limit,
            string? paginationToken)
        {
            string? nextToken = paginationToken;
            List<TodoItem> items = new();
            Dictionary<string, AttributeValue>? lastEvaluatedKey;

            do
            {
                QueryRequest request = DynamoQueryHelper.CreateUserIdQuery(
                    TableName,
                    userId,
                    filterExpression,
                    values,
                    limit,
                    nextToken,
                    scanIndexForward: false);

                QueryResponse response = await _client.QueryAsync(request);
                items = response.Items
                    .Select(item => _context.FromDocument<TodoItem>(Document.FromAttributeMap(item)))
                    .ToList();

                lastEvaluatedKey = response.LastEvaluatedKey;
                nextToken = DynamoQueryHelper.EncodePaginationToken(lastEvaluatedKey);
            }
            // dynamodb returns LastEvaluatedKey based on items it scanned, not on items that passed the filter
            // so we need to keep querying until we find at least one matching item or reach the end
            while (items.Count == 0 && lastEvaluatedKey != null && lastEvaluatedKey.Count > 0);

            return (items, nextToken);
        }

        // Build filter expression and values for status
        internal static (string FilterExpression, Dictionary<string, AttributeValue> Values) BuildStatusFilter(
            TodoStatus? status)
        {
            if (status.HasValue)
            {
                // return expression with given status
                return (
                    "StatusCode = :status",
                    new Dictionary<string, AttributeValue>
                    {
                        [":status"] = new AttributeValue { N = ((int)status.Value).ToString() }
                    }
                );
            }

            // return expression with all status
            return (
                "StatusCode = :incomplete OR StatusCode = :complete",
                new Dictionary<string, AttributeValue>
                {
                    [":incomplete"] = new AttributeValue { N = ((int)TodoStatus.Incomplete).ToString() },
                    [":complete"] = new AttributeValue { N = ((int)TodoStatus.Complete).ToString() }
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
            await _context.SaveAsync(todo);
            return true;
        }
    }
}
