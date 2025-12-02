using Interfaces.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.responses;

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

        /// <summary>
        /// Retrieves paginated alert history logs for a specific device.
        /// </summary>
        /// <remarks>
        /// Requires authorization under the 'AllRoles' policy. The logs are retrieved 
        /// using pagination parameters for efficiency. Returns an empty list if no logs are found.
        /// </remarks>
        /// <param name="deviceId">The ID of the device to fetch logs for (from URL path).</param>
        /// <param name="pageNumber">The page number for pagination (defaults to 1).</param>
        /// <param name="pageSize">The number of items per page (defaults to 25).</param>
        /// <returns>Returns 200 OK with a list of AlertLog objects.</returns>
        [Authorize(Policy = "AllRoles")]
        [HttpGet("get-device-alert-logs")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PagedResult<AlertLogResponse>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetDeviceAlertLogs(int deviceId, int pageNumber = 1, int pageSize = 25)
        {
            var logs = await _alertLogsManager.GetDeviceAlertLogs(deviceId, pageNumber, pageSize);
            return Ok(logs);
        }
    }
}
