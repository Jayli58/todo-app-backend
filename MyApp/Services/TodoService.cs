using MyApp.Common;
using MyApp.Data.Repos;
using MyApp.Exceptions;
using MyApp.Models.Dto;
using MyApp.Models.Entity;
using MyApp.Models.Enum;

namespace MyApp.Services
{
    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _repo;
        private readonly IReminderRepository _repoReminder;
        private readonly ICurrentUser _currentUser;

        private const int TITLE_MAX = 100;
        private const int CONTENT_MAX = 200;

        public TodoService(ITodoRepository repo, IReminderRepository repoReminder, ICurrentUser currentUser)
        {
            _repo = repo;
            _repoReminder = repoReminder;
            _currentUser = currentUser;
        }

        public async Task<TodoItem> CreateTodoAsync(CreateTodoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new BaseException("Title is required.");

            if (request.Title.Length > TITLE_MAX)
                throw new BaseException($"Title must be at most {TITLE_MAX} characters.");

            if (request.Content != null && request.Content.Length > CONTENT_MAX)
                throw new BaseException($"Content must be at most {CONTENT_MAX} characters.");

            TodoItem todo = new TodoItem
            {
                UserId = _currentUser.UserId,
                // Generate a new ULID for the TodoId
                TodoId = UlidGenerator.NewUlid(),
                Title = request.Title,
                Content = request.Content,
                StatusCode = TodoStatus.Incomplete,
                RemindTimestamp = null
            };

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

        public async Task<bool> SetRemainderAsync(string userId, string todoId, long remindTimestamp)
        {
            TodoItem existing = await _repo.GetTodoAsync(userId, todoId);
            // should not be null
            if (existing == null) return false;

            // update the remind timestamp
            existing.RemindTimestamp = remindTimestamp;
            await _repo.UpdateTodoAsync(existing);

            // create or update the reminder
            TodoReminder? existingReminder = await _repoReminder.GetByTodoAsync(userId, todoId);

            TodoReminder reminder;

            if (existingReminder != null)
            {
                reminder = existingReminder;
                reminder.RemindAtEpoch = remindTimestamp;
            }
            else
            {
                reminder = new TodoReminder
                {
                    UserId = userId,
                    TodoId = todoId,
                    Email = _currentUser.Email,
                    Title = existing.Title,
                    Content = existing.Content,                    
                    RemindAtEpoch = remindTimestamp
                };
            }
            await _repoReminder.UpsertAsync(reminder);

            return true;
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
