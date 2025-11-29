using System.Net;

namespace MyApp.Exceptions
{
    public class BaseException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public BaseException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
