using MyApp.Extensions;

namespace MyApp.Services
{
    public class CurrentUser : ICurrentUser
    {
        public string UserId { get; }
        public string Email { get; }
        public string? Name { get; }

        public CurrentUser(IHttpContextAccessor accessor)
        {
            var user = accessor.HttpContext?.User
                       ?? throw new UnauthorizedAccessException("No authenticated user.");

            UserId = user.GetUserId()
                ?? throw new UnauthorizedAccessException("UserId (sub) not found in token.");

            Email = user.GetEmail()
                ?? throw new UnauthorizedAccessException("Email not found in token.");
            Name = user.GetName();
        }
    }
}
