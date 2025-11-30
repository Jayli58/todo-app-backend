using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using MyApp.Data.Repos;
using MyApp.Services;

namespace MyApp.Extensions
{
    public static class DynamoDbExtensions
    {
        public static IServiceCollection AddDynamoDb(this IServiceCollection services, IConfiguration config)
        {
            var dynamoUrl = config["AWS:DynamoDB:ServiceURL"];
            var region = config["AWS:DynamoDB:Region"];
            var awsAccessKeyId = config["AWS:DynamoDB:AccessKeyId"];
            var awsSecretAccessKey = config["AWS:DynamoDB:SecretAccessKey"];

            //Console.WriteLine("DynamoDB URL: " + dynamoUrl);

            // DynamoDB client
            services.AddSingleton<IAmazonDynamoDB>(sp =>
            {
                return new AmazonDynamoDBClient(
                    awsAccessKeyId: awsAccessKeyId,
                    awsSecretAccessKey: awsSecretAccessKey,
                    new AmazonDynamoDBConfig
                    {
                        ServiceURL = dynamoUrl,
                        AuthenticationRegion = region,
                        UseHttp = true
                    }
                );
            });

            // DynamoDBContext wrapper
            services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

            // Repositories
            services.AddSingleton<ITodoRepository, DynamoTodoRepository>();

            // Services -- scoped per request, consistent to CurrentUser
            services.AddScoped<ITodoService, TodoService>();

            return services;
        }
    }
}
