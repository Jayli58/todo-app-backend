using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using System.Text.Json;

// PoC handler that logs DynamoDB event records
namespace RemainderLambda.Handlers
{
    public class DynamodbEventPocHandler
    {
        public DynamodbEventPocHandler() { }
        public Task HandleAsync(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            context.Logger.LogInformation($"Beginning to process {dynamoEvent.Records.Count} records...");

            foreach (var record in dynamoEvent.Records)
            {
                context.Logger.LogInformation($"Event ID: {record.EventID}");
                context.Logger.LogInformation($"Event Name: {record.EventName}");

                context.Logger.LogInformation(JsonSerializer.Serialize(record));
            }

            context.Logger.LogInformation("Stream processing complete.");

            return Task.CompletedTask;
        }
    }
}
