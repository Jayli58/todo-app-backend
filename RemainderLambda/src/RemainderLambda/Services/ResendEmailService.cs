using Amazon.Auth.AccessControlPolicy;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Resend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemainderLambda.Services
{
    public class ResendEmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly string _sender;

        public ResendEmailService(
            IResend resend,
            string sender
        )
        {
            _resend = resend;
            _sender = $"Todo App <{sender}>";
        }

        public async Task<string> SendEmailAsync(string to, string subject, string body)
        {
            // Resend API does not accept empty body
            var safeBody = string.IsNullOrWhiteSpace(body) ? "No content." : body;

            var msg = new EmailMessage();
            msg.From = _sender;
            msg.To.Add(to);
            msg.Subject = subject;
            msg.TextBody = safeBody;

            var response = await _resend.EmailSendAsync(msg);

            Console.WriteLine($"Email has been sent successfully with msg id of \n{response.Content}");
            return response.Content.ToString();
        }
    }


}
