using Amazon.DynamoDBv2.DataModel;
using MyApp.Models;

namespace MyApp.Data.Repos
{
    public class DynamoTodoRepository : ITodoRepository
    {
        private readonly IDynamoDBContext _context;

        public DynamoTodoRepository(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<TodoItem?> GetTodoAsync(string userId, string todoId)
        {
            return await _context.LoadAsync<TodoItem>(userId, todoId);
        }

        public async Task<IEnumerable<TodoItem>> GetAllTodosAsync(string userId)
        {
            return await _context.ScanAsync<TodoItem>(new List<ScanCondition>()).GetRemainingAsync();
        }

        public async Task AddTodoAsync(TodoItem todo)
        {
            await _context.SaveAsync(todo);
        }

        public async Task DeleteTodoAsync(string userId, string todoId)
        {
            await _context.DeleteAsync<TodoItem>(userId, todoId);
        }

        public async Task UpdateTodoAsync(TodoItem todo)
        {
            await _context.SaveAsync(todo);
        }
    }
}
