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
            // Query instead of Scan for better performance as we have the partition key (userId)
            var todos = await _context.QueryAsync<TodoItem>(userId).GetRemainingAsync();

            return status.HasValue
                ? todos.Where(t => t.StatusCode == status.Value)
                : todos.Where(t => t.StatusCode == TodoStatus.Incomplete || t.StatusCode == TodoStatus.Complete);
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
