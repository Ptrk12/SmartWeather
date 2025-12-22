using Interfaces.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.requests;
using Models.responses;

namespace SmartWeather.Controllers
{
    [Route("api/group/{groupId}/device/{deviceId}")]
    [ApiController]
    public class AlertController : ControllerBase
    {
        private readonly IAlertManager _alertManager;

        public AlertController(IAlertManager alertManager)
        {
            _alertManager = alertManager;
        }

        /// <summary>
        /// Creates a new alert rule for a specific sensor metric.
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'Admin' policy.
        /// The API returns the error message string directly upon a 409 conflict.
        /// </remarks>
        /// <param name="req">The data for the new alert rule.</param>
        /// <param name="groupId">The ID of the group (from URL path).</param>
        /// <param name="deviceId">The ID of the device (from URL path).</param>
        /// <param name="sensorMetricId">The ID of the sensor metric to attach the rule to (from URL path).</param>
        /// <returns>Returns 201 Created on success, or 409 Conflict with a message on failure.</returns>
        [Authorize(Policy = "Admin")]
        [HttpPost("sensor-metric/{sensorMetricId}/alert-rule/add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAlertRule(CreateAlertReq req, int groupId, int deviceId, int sensorMetricId)
        {
            var result = await _alertManager.CreateAlertRule(req, sensorMetricId,groupId);

            return result.Success == true ? Created() : Conflict(result.Message);
        }
        /// <summary>
        /// Updates an existing alert rule.
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'Admin' policy. Returns 201 Created upon success.
        /// </remarks>
        /// <param name="req">The updated data for the alert rule.</param>
        /// <param name="groupId">The ID of the group (from URL path).</param>
        /// <param name="deviceId">The ID of the device (from URL path).</param>
        /// <param name="sensorMetricId">The ID of the sensor metric (from URL path).</param>
        /// <param name="alertId">The ID of the alert rule to update (from URL path).</param>
        /// <returns>Returns 201 Created on success, or 409 Conflict with a message on failure.</returns>
        [Authorize(Policy = "Admin")]
        [HttpPut("sensor-metric/{sensorMetricId}/alert-rule/{alertId}/update")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAlertRule(CreateAlertReq req, int groupId, int deviceId, int sensorMetricId, int alertId)
        {
            var result = await _alertManager.EditAlertRule(req,sensorMetricId,alertId,groupId);

            return result.Success == true ? Created() : Conflict(result.Message);
        }

        /// <summary>
        /// Deletes a specific alert rule.
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'Admin' policy. 
        /// </remarks>
        /// <param name="groupId">The ID of the group (from URL path).</param>
        /// <param name="deviceId">The ID of the device (from URL path).</param>
        /// <param name="sensorMetricId">The ID of the sensor metric (from URL path).</param>
        /// <param name="alertId">The ID of the alert rule to delete (from URL path).</param>
        /// <returns>Returns 204 on success, or 409 Conflict with a message on failure.</returns>
        [Authorize(Policy = "Admin")]
        [HttpDelete("sensor-metric/{sensorMetricId}/alert-rule/{alertId}/delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] 
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteAlertRule(int groupId, int deviceId, int sensorMetricId, int alertId)
        {
            var result = await _alertManager.DeleteAlertRule(alertId,sensorMetricId, groupId);

            return result.Success == true ? NoContent() : Conflict(result.Message);
        }
        /// <summary>
        /// Retrieves all alert rules configured for a specific device.
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'AllRoles' policy. Returns an empty list if none are found.
        /// </remarks>
        /// <param name="groupId">The ID of the group (from URL path).</param>
        /// <param name="deviceId">The ID of the device (from URL path).</param>
        /// <returns>Returns 200 OK with a list of AlertRule objects.</returns>
        [Authorize(Policy = "AllRoles")]
        [HttpGet("get-device-alert-rules")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AlertRuleResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetDeviceAlertRules(int groupId, int deviceId)
        {
            var alertRules = await _alertManager.GetDeviceAlertRules(deviceId);
            return Ok(alertRules);
        }

        /// <summary>
        /// Retrieves a specific alert rule by its unique ID and associated sensor metric ID.
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'AllRoles' policy.
        /// </remarks>
        /// <param name="groupId">The ID of the group (from URL path).</param>
        /// <param name="deviceId">The ID of the device (from URL path).</param>
        /// <param name="sensorMetricId">The ID of the sensor metric (from URL path).</param>
        /// <param name="alertId">The unique ID of the alert rule to retrieve (from URL path).</param>
        /// <returns>Returns 200 OK with the AlertRule object on success, or 404 Conflict on failure.</returns>
        [Authorize(Policy = "AllRoles")]
        [HttpGet("sensor-metric/{sensorMetricId}/alert-rule/{alertId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AlertRuleResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAlertRuleById(int groupId, int deviceId,int sensorMetricId, int alertId)
        {
            var result = await _alertManager.GetAlertRuleById(sensorMetricId,alertId);
            return result.Success == true ? Ok(result.Data) : NotFound(result.Message);
        }     
    }
}
