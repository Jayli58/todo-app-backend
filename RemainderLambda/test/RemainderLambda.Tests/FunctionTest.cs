using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.TestUtilities;
using System.Text;
using Xunit;
using RemainderLambda.Handlers;

namespace RemainderLambda.Tests;

public class FunctionTest
{
    [Fact]
    public void TestDynamoDbStreamHandler()
    {
        // Arrange sample DynamoDB Stream REMOVE event
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
                  ""UserId"": { ""S"": ""USER-1"" },
                  ""TodoId"": { ""S"": ""TODO-1"" },
                  ""Email"": { ""S"": ""test@example.com"" },
                  ""Title"": { ""S"": ""Buy milk"" },
                  ""Content"": { ""S"": ""Remember to buy milk"" },
                  ""RemindAtEpoch"": { ""N"": ""1732885200"" }
                }
              },
              ""eventSourceARN"": ""arn:aws:dynamodb:ap-southeast-2:000000000000:table/TodoReminders/stream/2025-11-30T02:14:24.732""
            }
          ]
        }";

        // Use AWS Lambda serializer
        var serializer = new DefaultLambdaJsonSerializer();
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var dynamoEvent = serializer.Deserialize<DynamoDBEvent>(stream);

        var handler = new DynamodbEventPocHandler();
        var context = new TestLambdaContext();

        // Act
        handler.HandleAsync(dynamoEvent, context);

        // Capture logs
        var testLogger = (TestLambdaLogger)context.Logger;
        var logs = testLogger.Buffer.ToString();
        Console.WriteLine("=== LAMBDA LOGS ===");
        Console.WriteLine(logs);

        // Assert
        Assert.Contains("Beginning to process 1 records", logs);
    }
}
