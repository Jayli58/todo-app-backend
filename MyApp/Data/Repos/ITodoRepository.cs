using MyApp.Models.Entity;
using MyApp.Models.Enum;

namespace MyApp.Data.Repos
{
    public interface ITodoRepository
    {
        Task<TodoItem> GetTodoAsync(string userId, string todoId);
        // Paginated query of todos for a user
        Task<(IEnumerable<TodoItem> Items, string? NextToken)> QueryTodosPageAsync(
            string userId,
            TodoStatus? status,
            int limit,
            string? paginationToken);    
        // Paginated search of todos for a user
        Task<(IEnumerable<TodoItem> Items, string? NextToken)> SearchTodosPageAsync(
            string userId,
            string? query,
            int limit,
            string? paginationToken);
        Task<TodoItem> AddTodoAsync(TodoItem todo);
        Task<bool> DeleteTodoAsync(string userId, string todoId);
        Task UpdateTodoAsync(TodoItem todo);

        Task<bool> MarkAsDeletedAsync(string userId, string todoId);
    }
}
