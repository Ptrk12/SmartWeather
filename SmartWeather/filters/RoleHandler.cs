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
        private readonly ISensorMetricRepository _sensorMetricRepository;
        private readonly IUserManager _userManager;

        public RoleHandler(IHttpContextAccessor httpContextAccessor, IGroupRepository groupRepository,IUserManager userManager, IDeviceRepository deviceRepository, ISensorMetricRepository sensorMetricRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _groupRepository = groupRepository;
            _userManager = userManager;
            _deviceRepository = deviceRepository;
            _sensorMetricRepository = sensorMetricRepository;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
        {
            var httpContext = _httpContextAccessor.HttpContext!;
            var userId = _userManager.GetUserId(httpContext.User);

            if (string.IsNullOrEmpty(userId))
                return;

            var groupIdStr = httpContext.Request.RouteValues["groupId"]?.ToString()
                ?? httpContext.Request.Query["groupId"].ToString();

            var deviceIdStr = httpContext.Request.RouteValues["deviceId"]?.ToString()
                ?? httpContext.Request.Query["deviceId"].ToString();

            var sensorMetricStr = httpContext.Request.RouteValues["sensorMetricId"]?.ToString()
                ?? httpContext.Request.Query["sensorMetricId"].ToString();

            if (string.IsNullOrEmpty(groupIdStr) &&
                string.IsNullOrEmpty(deviceIdStr) &&
                string.IsNullOrEmpty(sensorMetricStr))
            {
                context.Succeed(requirement);
                return;
            }

            if (!int.TryParse(groupIdStr, out var groupId))
                return;

            if (!string.IsNullOrWhiteSpace(deviceIdStr))
            {
                if (!int.TryParse(deviceIdStr, out var deviceId))
                    return;

                var deviceOk = await _deviceRepository.IsDeviceAllowedForUser(userId, groupId, deviceId);
                if (!deviceOk)
                    return;

                if (!string.IsNullOrWhiteSpace(sensorMetricStr))
                {
                    if (!int.TryParse(sensorMetricStr, out var sensorMetricId))
                        return;

                    var sensorMetricOk = await _sensorMetricRepository.IsSensorMetricAllowedForUser(deviceId, sensorMetricId);
                    if (!sensorMetricOk)
                        return;
                }
            }

            var role = await _groupRepository.GetUserRoleInGroup(userId, groupId);
            if (!string.IsNullOrEmpty(role) && requirement.RequirementRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
            {
                context.Succeed(requirement);
            }
        }

    }
}
