using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using RemainderLambda.Models;
using RemainderLambda.Services;


namespace RemainderLambda.Handlers
{
    public class ReminderStreamHandler
    {
        private readonly SesEmailService _email;

        public ReminderStreamHandler(SesEmailService emailService)
        {
            _email = emailService;
        }

        public async Task HandleAsync(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            foreach (var record in dynamoEvent.Records)
            {
                if (record.EventName != "REMOVE")
                    continue;

                var img = record.Dynamodb.OldImage;
                if (img == null)
                    continue;

                var reminder = new ReminderRecord
                {
                    ReminderId = img["ReminderId"].S,
                    UserId = img["UserId"].S,
                    TodoId = img["TodoId"].S,
                    Email = img["Email"].S,
                    Title = img["Title"].S,
                    Content = img["Content"].S,
                    RemindAtEpoch = long.Parse(img["RemindAtEpoch"].N)
                };

                context.Logger.LogInformation($"[Lambda] Processing ReminderId={reminder.ReminderId} under UserId={reminder.UserId}");

                await _email.SendEmailAsync(
                    reminder.Email,
                    subject: reminder.Title,
                    body: reminder.Content
                );
            }
        }


    }
}
