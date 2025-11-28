using MyApp.Models.Enum;

namespace MyApp.Models.Dto
{
    public class CreateTodoRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }
}
