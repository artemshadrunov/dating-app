using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        // Gets username from auth token
        public static string GetUsername(this ClaimsPrincipal user)
        {            
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}