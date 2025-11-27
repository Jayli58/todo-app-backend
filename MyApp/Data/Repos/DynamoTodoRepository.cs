using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using MyApp.Models.Entity;
using MyApp.Models.Enum;

namespace MyApp.Data.Repos
{
    public class DynamoTodoRepository : ITodoRepository
    {
        private readonly IDynamoDBContext _context;

        public DynamoTodoRepository(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<TodoItem> GetTodoAsync(string userId, string todoId)
        {
            return await _context.LoadAsync<TodoItem>(userId, todoId);
        }

        public async Task<IEnumerable<TodoItem>> GetAllTodosAsync(string userId, TodoStatus? status)
        {
            var conditions = new List<ScanCondition>();

            // Build scan conditions based on whether status is provided
            if (status.HasValue)
            {
                conditions.Add(new ScanCondition("UserId", ScanOperator.Equal, userId));
                conditions.Add(new ScanCondition("StatusCode", ScanOperator.Equal, status.Value));
            }
            else
            {
                conditions.Add(new ScanCondition("UserId", ScanOperator.Equal, userId));
                // Exclude deleted items
                conditions.Add(new ScanCondition("StatusCode", ScanOperator.In, TodoStatus.Incomplete, TodoStatus.Complete));
            }

            return await _context.ScanAsync<TodoItem>(conditions).GetRemainingAsync();
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
    }
}
