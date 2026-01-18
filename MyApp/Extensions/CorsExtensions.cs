namespace MyApp.Extensions
{
    public static class CorsExtensions
    {
        private const string PolicyName = "AllowFrontend";

        public static IServiceCollection AddMyCors(this IServiceCollection services, IConfiguration config)
        {
            var frontendUrl = config["Frontend:Url"] ?? "http://localhost:3000";

            // Ensure localhost is always allowed during development and production
            var allowedOrigins = new[]
            {
                frontendUrl,
                "http://localhost:3000"
            }
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Distinct()
            .ToArray();


            services.AddCors(options =>
            {
                options.AddPolicy(PolicyName, builder =>
                {
                    builder
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            return services;
        }

        public static string GetPolicyName() => PolicyName;
    }
}
