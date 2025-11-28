using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace MyApp.Extensions
{
    public static class CognitoAuthExtensions
    {
        public static IServiceCollection AddCognitoAuth(this IServiceCollection services, IConfiguration config)
        {
            var region = config["Cognito:Region"];
            var userPoolId = config["Cognito:UserPoolId"];
            var clientId = config["Cognito:ClientId"];

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";
                    options.Audience = clientId;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,       // false to allow access tokens
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
