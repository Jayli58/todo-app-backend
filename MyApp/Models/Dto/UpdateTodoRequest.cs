using MyApp.Models.Enum;

namespace MyApp.Models.Dto
{
    public class UpdateTodoRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public long? RemindTimestamp { get; set; }
        public TodoStatus StatusCode { get; set; }
    }
}
