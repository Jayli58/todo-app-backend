using MyApp.Models.Dto;
using MyApp.Models.Entity;
using MyApp.Models.Enum;

namespace MyApp.Services
{
    public interface ITodoService
    {
        Task<(IEnumerable<TodoItem> Items, string? NextToken)> SearchTodosAsync(
            string userId,
            string? query,
            int limit,
            string? paginationToken);

        Task<TodoItem> GetTodoAsync(string userId, string todoId);

        Task<TodoItem> CreateTodoAsync(CreateTodoRequest request);

        Task<TodoItem?> UpdateTodoAsync(string userId, string todoId, UpdateTodoRequest request);

        Task<bool> DeleteTodoAsync(string userId, string todoId);

        Task<(IEnumerable<TodoItem> Items, string? NextToken)> GetTodosAsync(
            string userId,
            TodoStatus? status,
            int limit,
            string? paginationToken);

        Task<bool> SetRemainderAsync(string userId, string todoId, long RemindTimestamp);
    }
}
