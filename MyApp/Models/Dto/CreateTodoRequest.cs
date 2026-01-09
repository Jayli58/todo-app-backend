using MyApp.Models.Enum;

namespace MyApp.Models.Dto
{
    public class CreateTodoRequest
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
    }
}
