using System.Security.Claims;

namespace Interfaces.Managers
{
    public interface IUserManager
    {
        string GetUserEmail(ClaimsPrincipal user);
        string GetUserId(ClaimsPrincipal user);
    }
}
