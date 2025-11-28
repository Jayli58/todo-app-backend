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

            // DynamoDB client
            services.AddSingleton<IAmazonDynamoDB>(sp =>
            {
                return new AmazonDynamoDBClient(
                    new AmazonDynamoDBConfig
                    {
                        ServiceURL = dynamoUrl
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
