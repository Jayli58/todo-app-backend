using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using RemainderLambda.Models;
using RemainderLambda.Services;


namespace RemainderLambda.Handlers
{
    public class ReminderStreamHandler
    {
        private readonly IEmailService _email;

        public ReminderStreamHandler(IEmailService emailService)
        {
            _email = emailService;
        }

        public async Task HandleAsync(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            foreach (var record in dynamoEvent.Records)
            {
                try
                {
                    await ProcessRecordAsync(record, context);
                }
                catch (Exception ex)
                {
                    context.Logger.LogError($"[ERROR] Failed processing record {record.EventID}: {ex}");               
                }
            }
        }

        private async Task ProcessRecordAsync(DynamoDBEvent.DynamodbStreamRecord record, ILambdaContext context)
        {
            // ttl expiration leads to REMOVE events
            if (record.EventName != "REMOVE")
                return;

            // Extract old image only
            var img = record.Dynamodb.OldImage;
            if (img == null)
                return;

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

            context.Logger.LogInformation($"[Lambda] Processing ReminderId={reminder.ReminderId}");

            await _email.SendEmailAsync(
                reminder.Email,
                reminder.Title,
                reminder.Content
            );
        }

    }
}
