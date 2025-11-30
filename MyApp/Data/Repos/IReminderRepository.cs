using MyApp.Models.Entity;

namespace MyApp.Data.Repos
{
    public interface IReminderRepository
    {
        Task<TodoReminder> UpsertAsync(TodoReminder reminder);
        Task<TodoReminder?> GetByTodoAsync(string userId, string todoId);
    }
}
