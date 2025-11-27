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

        public TodoService(ITodoRepository repo)
        {
            _repo = repo;
        }

        public async Task<TodoItem> CreateTodoAsync(TodoItem todo)
        {
            // Generate a new ULID for the TodoId
            todo.TodoId = UlidGenerator.NewUlid();

            TodoItem insertedTodo = await _repo.AddTodoAsync(todo);

            return insertedTodo;
        }

        public async Task<bool> DeleteTodoAsync(string userId, string todoId)
        {
            return await _repo.DeleteTodoAsync(userId, todoId);
        }

        public async Task<TodoItem> GetTodoAsync(string userId, string todoId)
        {
            TodoItem todo = await _repo.GetTodoAsync(userId, todoId);

            return todo;
        }

        public async Task<IEnumerable<TodoItem>> GetTodosAsync(string userId, TodoStatus? status)
        {
            IEnumerable<TodoItem> allTodos = await _repo.GetAllTodosAsync(userId, status);
            
            return allTodos;
        }

        public async Task<IEnumerable<TodoItem>> SearchTodosAsync(string userId, string? query)
        {
            IEnumerable<TodoItem> allTodos = await _repo.GetAllTodosAsync(userId, null);

            if (string.IsNullOrWhiteSpace(query))
            {
                return allTodos;
            }

            query = query.ToLowerInvariant().Trim();

            return allTodos.Where(todo =>
                (todo.Title != null && todo.Title.ToLowerInvariant().Contains(query)) ||
                (todo.Content != null && todo.Content.ToLowerInvariant().Contains(query))
            );
        }

        public async Task<TodoItem> UpdateTodoAsync(string userId, string todoId, UpdateTodoRequest request)
        {
            TodoItem existing = await _repo.GetTodoAsync(userId, todoId);
            if (existing == null) return null;

            existing.Title = request.Title;
            existing.Content = request.Content;
            existing.Completed = request.Completed;
            existing.StatusCode = request.StatusCode;
            existing.RemindTimestamp = request.RemindTimestamp;

            await _repo.UpdateTodoAsync(existing);

            return existing;
        }
    }
}
