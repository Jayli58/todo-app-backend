using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.SimpleEmail;
using RemainderLambda.Handlers;
using RemainderLambda.Services;

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

        // Initialize the handler with the SES email service as entry point
        _handler = new ReminderStreamHandler(new SesEmailService(sesClient, sender));
    }

    // Lambda entrypoint
    public async Task FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        context.Logger.LogInformation($"[Function] Received {dynamoEvent.Records.Count} stream records");
        await _handler.HandleAsync(dynamoEvent, context);
        context.Logger.LogInformation("[Function] Done processing reminder records");
    }
}
