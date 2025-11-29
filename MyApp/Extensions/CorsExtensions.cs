namespace MyApp.Extensions
{
    public static class CorsExtensions
    {
        private const string PolicyName = "AllowFrontend";

        public static IServiceCollection AddMyCors(this IServiceCollection services, IConfiguration config)
        {
            var frontendUrl = config["Frontend:Url"] ?? "http://localhost:3000";

            services.AddCors(options =>
            {
                options.AddPolicy(PolicyName, builder =>
                {
                    builder
                        .WithOrigins(frontendUrl)
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
