using RemainderLambda.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemainderLambda.Tests
{
    internal class FakeSuccessEmailService : IEmailService
    {
        public Task<string> SendEmailAsync(string to, string subject, string body)
        {
            // Simulate successful send
            return Task.FromResult("fake-message-id");
        }
    }
}
