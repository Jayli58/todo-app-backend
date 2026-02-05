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

            string todoId = UlidGenerator.NewUlid();
            TodoItem todo = new TodoItem
            {
                UserId = _currentUser.UserId,
                // Generate a new ULID for the TodoId
                TodoId = todoId,
                Title = request.Title,
                Content = request.Content,
                // store lower case for case insensitive search
                TitleLower = request.Title.ToLowerInvariant(),
                ContentLower = request.Content?.ToLowerInvariant(),
                StatusCode = TodoStatus.Incomplete,
                RemindTimestamp = null,
                ActiveTodoId = todoId
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

        public async Task<(IEnumerable<TodoItem> Items, string? NextToken)> GetTodosAsync(
            string userId,
            TodoStatus? status,
            int limit,
            string? paginationToken)
        {
            int safeLimit = Math.Max(0, limit);
            if (safeLimit == 0)
            {
                return (Array.Empty<TodoItem>(), null);
            }

            return await _repo.QueryTodosPageAsync(userId, status, safeLimit, paginationToken);
        }

        public async Task<(IEnumerable<TodoItem> Items, string? NextToken)> SearchTodosAsync(
            string userId,
            string? query,
            int limit,
            string? paginationToken)
        {
            int safeLimit = Math.Max(0, limit);
            if (safeLimit == 0)
            {
                return (Array.Empty<TodoItem>(), null);
            }

            return await _repo.SearchTodosPageAsync(userId, query, safeLimit, paginationToken);
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
            {
                existing.Title = request.Title;
                existing.TitleLower = request.Title.ToLowerInvariant();
            }

            if (request.Content != null)
            {
                existing.Content = request.Content;
                existing.ContentLower = request.Content.ToLowerInvariant();
            }

            if (request.StatusCode.HasValue)
            {
                existing.StatusCode = request.StatusCode.Value;
                existing.ActiveTodoId = existing.StatusCode == TodoStatus.Deleted
                    ? null
                    : existing.TodoId;
            }

            if (request.RemindTimestamp.HasValue)
                existing.RemindTimestamp = request.RemindTimestamp.Value;

            await _repo.UpdateTodoAsync(existing);

            return existing;
        }
    }
}
