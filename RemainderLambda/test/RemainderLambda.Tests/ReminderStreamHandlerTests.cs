using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.TestUtilities;
using Amazon.SimpleEmail;
using RemainderLambda.Handlers;
using RemainderLambda.Services;
using System.Text;
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
                          ""UserId"": { ""S"": ""USER-1"" },
                          ""TodoId"": { ""S"": ""TODO-1"" }
                        },
                        ""OldImage"": {
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
            // Create SES client using environment variables for configuration
            EnvLoader.LoadDotEnv();

            //var serviceUrl = Environment.GetEnvironmentVariable("SES_SERVICE_URL");
            var serviceUrl = LocalstackEndpointResolver.ResolveSesServiceUrl();
            var authRegion = Environment.GetEnvironmentVariable("SES_AUTH_REGION");
            var sender = Environment.GetEnvironmentVariable("SES_SENDER")
                         ?? throw new InvalidOperationException("Missing SES_SENDER");

            var sesConfig = new AmazonSimpleEmailServiceConfig
            {
                ServiceURL = serviceUrl,
                UseHttp = true,
                // important!!!
                AuthenticationRegion = authRegion
            };

            var sesClient = new AmazonSimpleEmailServiceClient(sesConfig);

            var handler = new ReminderStreamHandler(new SesEmailService(sesClient, sender));
            var context = new TestLambdaContext();

            // Act
            await handler.HandleAsync(dynamoEvent, context);

            // Get logs
            var logs = ((TestLambdaLogger) context.Logger).Buffer.ToString();
            Console.WriteLine(logs);

            // Assert
            Assert.Contains("Processing TodoId=TODO-1", logs);
            // check if email was "sent"
            Assert.Contains("Email sent for TodoId", logs);
        }

        [Fact]
        public async Task HandleAsync_Should_Not_Throw_Error_When_Email_Fails()
        {
            // Arrange failing SES mock
            var handler = new ReminderStreamHandler(new FakeFailingEmailService());
            var context = new TestLambdaContext();

            // Minimal REMOVE event
            var json = @"
            {
              ""Records"": [
                {
                  ""eventName"": ""REMOVE"",
                  ""dynamodb"": {
                    ""OldImage"": {
                      ""UserId"": { ""S"": ""USER-X"" },
                      ""TodoId"": { ""S"": ""TODO-X"" },
                      ""Email"": { ""S"": ""foo@bar.com"" },
                      ""Title"": { ""S"": ""Test"" },
                      ""Content"": { ""S"": ""Testing"" },
                      ""RemindAtEpoch"": { ""N"": ""1732885200"" }
                    }
                  }
                }
              ]
            }";

            var serializer = new DefaultLambdaJsonSerializer();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var dynamoEvent = serializer.Deserialize<DynamoDBEvent>(stream);

            // Act — This should NOT throw due to try/catch inside handler
            await handler.HandleAsync(dynamoEvent, context);

            // Assert
            var logs = ((TestLambdaLogger)context.Logger).Buffer.ToString();
            Assert.Contains("ERROR", logs);
            Assert.Contains("TODO-X", logs);
        }
    }
}
