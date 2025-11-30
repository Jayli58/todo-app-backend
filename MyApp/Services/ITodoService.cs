using MyApp.Models.Dto;
using MyApp.Models.Entity;
using MyApp.Models.Enum;

namespace MyApp.Services
{
    public interface ITodoService
    {
        Task<IEnumerable<TodoItem>> SearchTodosAsync(string userId, string? query);

        Task<TodoItem> GetTodoAsync(string userId, string todoId);

        Task<TodoItem> CreateTodoAsync(CreateTodoRequest request);

        Task<TodoItem?> UpdateTodoAsync(string userId, string todoId, UpdateTodoRequest request);

        Task<bool> DeleteTodoAsync(string userId, string todoId);

        Task<IEnumerable<TodoItem>> GetTodosAsync(string userId, TodoStatus? status);

        Task<bool> SetRemainderAsync(string userId, string todoId, long RemindTimestamp);
    }
}
