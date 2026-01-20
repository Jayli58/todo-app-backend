using System.Net;

namespace MyApp.Exceptions
{
    public class ReminderTimeInPastException : BaseException
    {
        public ReminderTimeInPastException() : base("Reminder time must be in the future.", HttpStatusCode.BadRequest)
        {
        }
    }
}
