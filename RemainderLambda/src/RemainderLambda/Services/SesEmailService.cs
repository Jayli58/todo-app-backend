using Amazon.Auth.AccessControlPolicy;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemainderLambda.Services
{
    public class SesEmailService : IEmailService
    {
        private readonly IAmazonSimpleEmailService _amazonSimpleEmailService;
        private readonly string _sender;

        public SesEmailService(
            IAmazonSimpleEmailService amazonSimpleEmailService,
            string sender
        )
        {
            _amazonSimpleEmailService = amazonSimpleEmailService;
            _sender = $"Todo App <{sender}>";
        }

        public async Task<string> SendEmailAsync(string to, string subject, string body)
        {
            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine("\n===== SES SEND SIMULATION =====");
            //Console.ResetColor();

            //Console.WriteLine($"To: {to}");
            //Console.WriteLine($"Subject: {subject}");
            //Console.WriteLine($"Body:\n{body}");
            //Console.WriteLine("===============================\n");

            //return Task.CompletedTask;

            //Console.WriteLine($"[SES] Using sender(Source) = {_sender}");

            var response = await _amazonSimpleEmailService.SendEmailAsync(
                new SendEmailRequest
                {
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { to }
                    },
                    Message = new Message
                    {
                        Subject = new Content
                        {
                            Charset = "UTF-8",
                            Data = subject
                        },
                        Body = new Body
                        {
                            Text = new Content
                            {
                                Charset = "UTF-8",
                                Data = body
                            }
                        }                        
                    },
                    Source = _sender
                });
            
            Console.WriteLine($"Email has been sent successfully with msg id of \n{response.MessageId}");
            return response.MessageId;
        }
    }


}
