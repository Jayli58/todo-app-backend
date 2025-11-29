using MyApp.Models.Entity;
using MyApp.Models.Enum;

namespace MyApp.Data.Repos
{
    public interface ITodoRepository
    {
        Task<TodoItem> GetTodoAsync(string userId, string todoId);
        Task<IEnumerable<TodoItem>> GetAllTodosAsync(string userId, TodoStatus? status);
        Task<TodoItem> AddTodoAsync(TodoItem todo);
        Task<bool> DeleteTodoAsync(string userId, string todoId);
        Task UpdateTodoAsync(TodoItem todo);

        Task<bool> MarkAsDeletedAsync(string userId, string todoId);
    }
}
