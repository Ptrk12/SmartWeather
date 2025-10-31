using Interfaces.Managers;
using System.Security.Claims;

namespace Managers
{
    public class UserManager : IUserManager
    {
        public string GetUserEmail(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        }

        public string GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }
    }
}
