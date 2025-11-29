using MyApp.Common;
using MyApp.Data.Repos;
using MyApp.Models.Dto;
using MyApp.Models.Entity;
using MyApp.Models.Enum;

namespace MyApp.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _repo;
        private readonly ICurrentUser _currentUser;

        public TodoService(ITodoRepository repo, ICurrentUser currentUser)
        {
            _repo = repo;
            _currentUser = currentUser;
        }

        public async Task<TodoItem> CreateTodoAsync(CreateTodoRequest request)
        {
            TodoItem todo = new TodoItem
            {
                UserId = _currentUser.UserId,
                Title = request.Title,
                Content = request.Content,
                StatusCode = TodoStatus.Incomplete,
                RemindTimestamp = null
            };

            // Generate a new ULID for the TodoId
            todo.TodoId = UlidGenerator.NewUlid();

            TodoItem insertedTodo = await _repo.AddTodoAsync(todo);

            return insertedTodo;
        }

        public async Task<bool> DeleteTodoAsync(string userId, string todoId)
        {
            return await _repo.MarkAsDeletedAsync(userId, todoId);
        }

        public async Task<TodoItem> GetTodoAsync(string userId, string todoId)
        {
            TodoItem todo = await _repo.GetTodoAsync(userId, todoId);

            return todo;
        }

        public async Task<IEnumerable<TodoItem>> GetTodosAsync(string userId, TodoStatus? status)
        {
            IEnumerable<TodoItem> allTodos = await _repo.GetAllTodosAsync(userId, status);

            // Return todos ordered by TodoId in descending order (newest first)
            return allTodos.OrderByDescending(t => t.TodoId);
        }

        public async Task<IEnumerable<TodoItem>> SearchTodosAsync(string userId, string? query)
        {
            IEnumerable<TodoItem> allTodos = await _repo.GetAllTodosAsync(userId, null);

            if (string.IsNullOrWhiteSpace(query))
            {
                // descending order (newest first)
                return allTodos.OrderByDescending(t => t.TodoId);
            }

            query = query.ToLowerInvariant().Trim();

            // Filter todos where Title or Content contains the query string (case-insensitive)
            // descending order (newest first)
            return allTodos.Where(todo =>
                (todo.Title != null && todo.Title.ToLowerInvariant().Contains(query)) ||
                (todo.Content != null && todo.Content.ToLowerInvariant().Contains(query))
            ).OrderByDescending(t => t.TodoId);
        }

        public async Task<TodoItem?> UpdateTodoAsync(string userId, string todoId, UpdateTodoRequest request)
        {
            TodoItem existing = await _repo.GetTodoAsync(userId, todoId);
            if (existing == null) return null;

            // Update only fields that the client actually sent
            if (request.Title != null)
                existing.Title = request.Title;

            if (request.Content != null)
                existing.Content = request.Content;

            if (request.StatusCode.HasValue)
                existing.StatusCode = request.StatusCode.Value;

            if (request.RemindTimestamp.HasValue)
                existing.RemindTimestamp = request.RemindTimestamp.Value;

            await _repo.UpdateTodoAsync(existing);

            return existing;
        }
    }
}
