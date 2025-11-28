using System.Security.Claims;

namespace MyApp.Extensions
{
    public static class UserContext
    {
        // Pretend this method belongs to ClaimsPrincipal
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static string? GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value;
        }

        public static string? GetName(this ClaimsPrincipal user)
        {
            return user.FindFirst("name")?.Value;
        }
    }
}
