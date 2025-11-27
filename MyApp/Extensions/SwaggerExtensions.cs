namespace MyApp.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddMySwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(c =>
            {
                c.UseInlineDefinitionsForEnums();    // Configure Swagger to use inline definitions for enums
            });

            return services;
        }
    }
}
