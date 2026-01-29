
using Interfaces.Managers;
using Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Security;

namespace AiChat.Plugins
{
    public class AiChatPlugin
        (
            IGroupManager groupManager,
            IDeviceManager deviceManager,
            IUserManager userManager,
            IGroupRepository groupRepository,
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

            var result = string.Join("\n", groups.Select(x => $"- ID: {x.Id} | Name: {x.Name} | Devices: {x.NumberOfDevices} | CreatedAt: {x.CreatedAt.Date}"));

            return "Your groups are:\n" + result;
        }

        [KernelFunction]
        [Description("Retrieves a detailed list of all physical devices and their current statuses (including alerts, serial number, status, last measurement and location) for a specific group")]
        public async Task<string> GetDevicesInGroup([Description("The ID of the group to fetch devices from")] int groupId)
        {
            var currentUserId = GetCurrentUserId();

            var role = await groupRepository.GetUserRoleInGroup(currentUserId, groupId);

            if(string.IsNullOrEmpty(role))
            {
                return "You do not have access to this group";
            }

            var devices = await deviceManager.GetDevicesAsync(groupId);

            if (!devices.Any())
            {
                return "No devices found in this group.";
            }

            var result = string.Join("\n\n", devices.Select(x =>
            $"Serial number: {x.SerialNumber}\n" +
            $"Location: {x.Location}\n" +
            $"Status: {x.Status}\n" +
            $"Current Alerts: {string.Join(", ", x.AlertStatuses.Select(a => $"Alert Message: {a.AlertMessage} Alert Sensor type: {a.SensorType}"))}\n" +
            $"Last Update: {x.LastMeasurement}"));

            return result;
        }

        private string GetCurrentUserId()
        {
            var user = httpContextAccessor.HttpContext?.User;

            if (user == null)
            {
                throw new SecurityException("Missing user context");
            }

            var userId = userManager.GetUserId(user);

            if (string.IsNullOrEmpty(userId))
            {
                throw new SecurityException("Unable to determine current user");
            }

            return userId;
        }
    }
}
