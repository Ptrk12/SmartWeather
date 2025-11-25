using Interfaces.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartWeather.Controllers
{
    [Route("api/groups/{groupId}/devices/{deviceId}")]
    [ApiController]
    public class AlertLogsController : ControllerBase
    {
        private readonly IAlertLogsManager _alertLogsManager;

        public AlertLogsController(IAlertLogsManager alertLogsManager)
        {
            _alertLogsManager = alertLogsManager;
        }

        [Authorize(Policy = "AllRoles")]
        [HttpGet("get-device-alert-logs")]
        public async Task<IActionResult> GetDeviceAlertLogs(int deviceId, int pageNumber = 1, int pageSize = 25)
        {
            var logs = await _alertLogsManager.GetDeviceAlertLogs(deviceId, pageNumber, pageSize);
            return Ok(logs);
        }
    }
}
