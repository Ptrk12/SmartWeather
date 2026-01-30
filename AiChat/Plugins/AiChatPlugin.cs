
using Interfaces.Managers;
using Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.SemanticKernel;
using Models.responses;
using Qdrant.Client.Grpc;
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
            IDeviceRepository deviceRepository,
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

            if (string.IsNullOrEmpty(role))
            {
                return "You do not have access to this group";
            }

            var devices = await deviceManager.GetDevicesAsync(groupId);

            if (!devices.Any())
            {
                return "No devices found in this group.";
            }

            var result = string.Join("\n\n", devices.Select(x =>
            $"- Device ID: {x.Id}\n" +
            $"- Serial number: {x.SerialNumber}\n" +
            $"- Location: {x.Location}\n" +
            $"- Status: {x.Status}\n" +
            $"- Current Alerts: {string.Join(", ", x.AlertStatuses.Select(a => $"Alert Message: {a.AlertMessage} Alert Sensor type: {a.SensorType}"))}\n" +
            $"- Last Update: {x.LastMeasurement}"));

            return result;
        }

        [KernelFunction]
        [Description("Retrieves historical measurement data for a specific device sensor (e.g., temperature, humidity, pressure, pm2_5, pm10)")]
        public async Task<string> GetDeviceMeasurements(
            [Description("The ID of the group to which the device belongs")] int groupId,
            [Description("The unique ID of the device")] int deviceId,
            [Description("The sensor parameter name. Supported values: 'temperature', 'humidity', 'pressure', 'pm2_5', 'pm10'")] string parameter)
        {
            var currentUserId = GetCurrentUserId();

            var isAllowed = await deviceRepository.IsDeviceAllowedForUser(currentUserId, groupId, deviceId);

            if (!isAllowed)
            {
                return "You do not have access to this device";
            }

            var measurements = await deviceManager.GetDeviceMeasurementAsync(deviceId, parameter, null, null);

            if (!measurements.Measurements.Any())
            {
                return "No measurements found for this device";
            }

            var formattedData = measurements.Measurements.Select(x => $"- Time: {x.Key.ToString("yyyy-MM-dd HH:mm")} | Value: {x.Value}");

            var resultString = string.Join("\n", formattedData);

            return $"History for {parameter}:\n{resultString}";
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
