using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemainderLambda.Services
{
    public class SesEmailService : IEmailService
    {
        public Task SendEmailAsync(string to, string subject, string body)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n===== SES SEND SIMULATION =====");
            Console.ResetColor();

            Console.WriteLine($"To: {to}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body:\n{body}");
            Console.WriteLine("===============================\n");

            return Task.CompletedTask;
        }
    }
}
