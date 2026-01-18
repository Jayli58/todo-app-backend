using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.SimpleEmail.Model;
using RemainderLambda.Models;
using RemainderLambda.Services;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;


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
                // content might be null
                UserId = GetStringAttr(img, "UserId"),
                TodoId = GetStringAttr(img, "TodoId"),
                Email = GetStringAttr(img, "Email"),
                Title = GetStringAttr(img, "Title"),
                Content = GetStringAttr(img, "Content"),
                RemindAtEpoch = long.Parse(img["RemindAtEpoch"].N)
            };

            context.Logger.LogInformation($"[Lambda] Processing TodoId={reminder.TodoId}");

            string messageId = await _email.SendEmailAsync(
                reminder.Email,
                reminder.Title,
                reminder.Content
            );

            context.Logger.LogInformation(
                $"Email sent for TodoId={reminder.TodoId}, MessageId={messageId}"
            );
        }

        // Helper to get string attribute from DynamoDB image as content might be null
        private static string GetStringAttr(
            Dictionary<string, AttributeValue> image,
            string key
            )
        {
            if (image == null) return string.Empty;
            // attributeValue.S -- DynamoDB string value
            return image.TryGetValue(key, out var attributeValue) ? (attributeValue.S ?? string.Empty) : string.Empty;
        }

    }
}
