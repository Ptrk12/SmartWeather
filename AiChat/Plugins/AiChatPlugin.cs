
using Interfaces.Managers;
using Microsoft.AspNetCore.Http;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Security;

namespace AiChat.Plugins
{
    public class AiChatPlugin
        (
            IGroupManager groupManager,
            IUserManager userManager,
            IHttpContextAccessor httpContextAccessor
        )
    {
        [KernelFunction]
        [Description("Get current user groups with devices count")]
        public async Task<string> GetUserGroups()
        {
            var userId = GetCurrentUserId();

            var groups = await groupManager.GetAllGroupsAsync(userId);

            if (!groups.Any())
            {
                return "You have no groups";
            }

            var result = string.Join("\n", groups.Select(x => $"- {x.Name}: {x.NumberOfDevices} devices (Description: {x.Description}, Created At: {x.CreatedAt.Date})"));

            return "Your groups are:\n" + result;
        }

        private string GetCurrentUserId()
        {
            var user = httpContextAccessor.HttpContext?.User;

            if(user == null)
            {
                throw new SecurityException("Missing user context");
            }

            var userId = userManager.GetUserId(user);

            if(string.IsNullOrEmpty(userId))
            {
                throw new SecurityException("Unable to determine current user");
            }

            return userId;
        }
    }
}
