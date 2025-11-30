using Amazon.DynamoDBv2.DataModel;
using MyApp.Models.Entity;

namespace MyApp.Data.Repos
{
    public class DynamoTodoReminderRepository : IReminderRepository
    {
        private readonly IDynamoDBContext _context;

        public DynamoTodoReminderRepository(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<TodoReminder?> GetByTodoAsync(string userId, string todoId)
        {
            return await _context.LoadAsync<TodoReminder>(todoId, userId);
        }

        public async Task<TodoReminder> UpsertAsync(TodoReminder reminder)
        {
            await _context.SaveAsync(reminder);
            return reminder;
        }
    }
}
