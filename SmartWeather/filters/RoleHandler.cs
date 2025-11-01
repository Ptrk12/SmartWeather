using Interfaces.Managers;
using Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace SmartWeather.filters
{
    public class RoleHandler : AuthorizationHandler<RoleRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGroupRepository _groupRepository;
        private readonly IUserManager _userManager;

        public RoleHandler(IHttpContextAccessor httpContextAccessor, IGroupRepository groupRepository,IUserManager userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _groupRepository = groupRepository;
            _userManager = userManager;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext!;
            var userId = _userManager.GetUserId(httpContext.User);

            if (string.IsNullOrEmpty(userId))
                return;

            var groupIdStr = httpContext.Request.RouteValues["groupId"]?.ToString() ?? httpContext.Request.Query["groupId"].ToString();

            if (!int.TryParse(groupIdStr, out var groupId))
                return;

            var role = await _groupRepository.GetUserRoleInGroup(userId,groupId);
            if (!string.IsNullOrEmpty(role) && requirement.RequirementRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }
        }
    }
}
