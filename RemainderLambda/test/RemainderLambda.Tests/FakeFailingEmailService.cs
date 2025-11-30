using RemainderLambda.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemainderLambda.Tests
{
    public class FakeFailingEmailService : IEmailService
    {
        public Task SendEmailAsync(string to, string subject, string body)
        {
            throw new Exception("Simulated SES failure");
        }
    }
}
