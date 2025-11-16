using Interfaces.Managers;
using Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace SmartWeather.filters
{
    public class RoleHandler : AuthorizationHandler<RoleRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGroupRepository _groupRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IUserManager _userManager;

        public RoleHandler(IHttpContextAccessor httpContextAccessor, IGroupRepository groupRepository,IUserManager userManager, IDeviceRepository deviceRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _groupRepository = groupRepository;
            _userManager = userManager;
            _deviceRepository = deviceRepository;
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

            var deviceIdStr = httpContext.Request.RouteValues["deviceId"]?.ToString()
                  ?? httpContext.Request.Query["deviceId"].ToString();

            if (!string.IsNullOrWhiteSpace(deviceIdStr))
            {
                if (!int.TryParse(deviceIdStr, out var deviceId))
                    return;

                var deviceOk = await _deviceRepository.IsDeviceAllowedForUser(userId, groupId, deviceId);
                if (!deviceOk)
                    return;
            }

            var role = await _groupRepository.GetUserRoleInGroup(userId,groupId);
            if (!string.IsNullOrEmpty(role) && requirement.RequirementRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }
        }
    }
}
