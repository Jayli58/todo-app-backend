using System.Net;

namespace MyApp.Exceptions
{
    public class TodoNotFoundException : BaseException
    {
        public TodoNotFoundException() : base("Todo not found.", HttpStatusCode.NotFound)
        {
        }
    }
}
