using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.SimpleEmail;
using RemainderLambda.Handlers;
using RemainderLambda.Services;
using System.Text.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RemainderLambda;

public class Function
{
    private readonly ReminderStreamHandler _handler;

    public Function()
    {
        // Create SES client using environment variables for configuration
        EnvLoader.LoadDotEnv();

        var serviceUrl = LocalstackEndpointResolver.ResolveSesServiceUrl();
        var sender = Environment.GetEnvironmentVariable("SES_SENDER")
                     ?? throw new InvalidOperationException("Missing SES_SENDER");

        var sesConfig = new AmazonSimpleEmailServiceConfig();
        
        // LocalStack mode
        if (!string.IsNullOrWhiteSpace(serviceUrl))
        {
            var authRegion = Environment.GetEnvironmentVariable("SES_AUTH_REGION")
                             ?? "ap-southeast-2";

            sesConfig.ServiceURL = serviceUrl;
            sesConfig.UseHttp = true;
            sesConfig.AuthenticationRegion = authRegion;
        }
        // else: AWS mode (real SES endpoint, no ServiceURL/UseHttp needed)

        var sesClient = new AmazonSimpleEmailServiceClient(sesConfig);

        // Initialize the handler with the SES email service as entry point
        _handler = new ReminderStreamHandler(new SesEmailService(sesClient, sender));
    }

    // Lambda entrypoint
    public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        context.Logger.LogInformation($"[Function] Received {dynamoEvent.Records.Count} stream records");

        // Log full event as JSON
        //var json = JsonSerializer.Serialize(dynamoEvent, new JsonSerializerOptions
        //{
        //    WriteIndented = true
        //});
        //context.Logger.LogInformation($"[Function] Dynamo db event JSON:\n{json}");

        await _handler.HandleAsync(dynamoEvent, context);
        context.Logger.LogInformation("[Function] Done processing reminder records");
    }
}
