using MyApp.Models;

namespace MyApp.Data.Repos
{
    public interface ITodoRepository
    {
        Task<TodoItem?> GetTodoAsync(string userId, string todoId);
        Task<IEnumerable<TodoItem>> GetAllTodosAsync(string userId);
        Task AddTodoAsync(TodoItem todo);
        Task DeleteTodoAsync(string userId, string todoId);
        Task UpdateTodoAsync(TodoItem todo);
    }
}
