using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.TestUtilities;
using RemainderLambda.Handlers;
using RemainderLambda.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RemainderLambda.Tests
{
    public class ReminderStreamHandlerTests
    {
        [Fact]
        public async Task Test_ReminderStreamHandler_Processes_Remove_Event()
        {
            // Arrange the DynamoDB REMOVE event JSON
            var json = @"
                {
                  ""Records"": [
                    {
                      ""eventID"": ""1"",
                      ""eventName"": ""REMOVE"",
                      ""eventVersion"": ""1.1"",
                      ""eventSource"": ""aws:dynamodb"",
                      ""awsRegion"": ""ap-southeast-2"",
                      ""dynamodb"": {
                        ""Keys"": {
                          ""ReminderId"": { ""S"": ""REM-123"" }
                        },
                        ""OldImage"": {
                          ""ReminderId"": { ""S"": ""REM-123"" },
                          ""UserId"":    { ""S"": ""USER-1"" },
                          ""TodoId"":    { ""S"": ""TODO-1"" },
                          ""Email"":     { ""S"": ""test@example.com"" },
                          ""Title"":     { ""S"": ""Buy milk"" },
                          ""Content"":   { ""S"": ""Remember to buy milk"" },
                          ""RemindAtEpoch"": { ""N"": ""1732885200"" }
                        }
                      }
                    }
                  ]
                }";

            // Deserialize JSON → DynamoDBEvent
            var serializer = new DefaultLambdaJsonSerializer();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var dynamoEvent = serializer.Deserialize<DynamoDBEvent>(stream);

            // Create handler + SES mock
            var handler = new ReminderStreamHandler(new SesEmailService());
            var context = new TestLambdaContext();

            // Act
            await handler.HandleAsync(dynamoEvent, context);

            // Get logs
            var logs = ((TestLambdaLogger)context.Logger).Buffer.ToString();
            Console.WriteLine(logs);

            // Assert
            Assert.Contains("REM-123", logs);
            Assert.Contains("USER-1", logs);
        }
    }
}
