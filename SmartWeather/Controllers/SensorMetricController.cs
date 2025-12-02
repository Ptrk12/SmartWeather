using Interfaces.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.requests;
using Models.responses;

namespace SmartWeather.Controllers
{
    [Route("api/group/{groupId}/device/{deviceId}/sensor-metric")]
    [ApiController]
    public class SensorMetricController : ControllerBase
    {
        private readonly ISensorMetricManager _sensorMetricManager;
        public SensorMetricController(ISensorMetricManager sensorMetricManager)
        {
            _sensorMetricManager = sensorMetricManager;
        }

        /// <summary>
        /// Adds a new sensor metric to the specified device.
        /// SENSOR TYPES:
        ///Temperature,
       /// Humidity,
       /// Pressure,
       /// Dust
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'Admin' policy.
        /// </remarks>
        /// <param name="req">The data for the new sensor metric.</param>
        /// <param name="deviceId">The ID of the device (from URL path).</param>
        /// <returns>Returns 201 Created on success, or 409 Conflict with a message on failure.</returns>
        [Authorize(Policy = "Admin")]
        [HttpPost("add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(string))] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddSensorMetric(CreateSensorMetric req, int deviceId)
        {
            var result = await _sensorMetricManager.AddSensorMetricAsync(req, deviceId);
            return result.Success == true ? Created() : Conflict(result.Message);
        }

        /// <summary>
        /// Updates an existing sensor metric.
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'Admin' policy.
        /// Returns 204 No Content for a successful update.
        /// </remarks>
        /// <param name="req">The updated data for the sensor metric.</param>
        /// <param name="deviceId">The ID of the device (from URL path).</param>
        /// <param name="sensorMetricId">The ID of the sensor metric to update (from URL path).</param>
        /// <param name="groupId">The ID of the group (from URL path).</param>
        /// <returns>Returns 204 NoContent on success, or 409 Conflict with a message on failure.</returns>
        [Authorize(Policy = "Admin")]
        [HttpPut("{sensorMetricId}/update")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSensorMetric(CreateSensorMetric req, int deviceId, int sensorMetricId, int groupId)
        {
            var result = await _sensorMetricManager.UpdateSensorMetricAsync(deviceId,sensorMetricId,req);
            return result.Success == true ? NoContent() : Conflict(result.Message);
        }

        /// <summary>
        /// Deletes a specific sensor metric.
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'Admin' policy.
        /// </remarks>
        /// <param name="deviceId">The ID of the device (from URL path).</param>
        /// <param name="sensorMetricId">The ID of the sensor metric to delete (from URL path).</param>
        /// <param name="groupId">The ID of the group (from URL path).</param>
        /// <returns>Returns 204 NoContent on success, or 409 Conflict with a message on failure.</returns>
        [Authorize(Policy = "Admin")]
        [HttpDelete("{sensorMetricId}/delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(string))] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteSenroMetric(int deviceId, int sensorMetricId, int groupId)
        {
            var result = await _sensorMetricManager.DeleteSensorMetricAsync(deviceId,sensorMetricId);
            return result.Success == true ? NoContent() : Conflict(result.Message);
        }

        /// <summary>
        /// Retrieves all sensor metrics associated with a specific device.
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'AllRoles' policy. Returns an empty list if none are found.
        /// </remarks>
        /// <param name="deviceId">The ID of the device (from URL path).</param>
        /// <param name="groupId">The ID of the group (from URL path).</param>
        /// <returns>Returns 200 OK with a list of SensorMetricResponse objects.</returns>
        [Authorize(Policy = "AllRoles")]
        [HttpGet("get-all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SensorMetricResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult > GetAllSensorMetrics(int deviceId, int groupId)
        {
            var sensorMetrics = await _sensorMetricManager.GetSensorMetricsAsync(deviceId);
            return Ok(sensorMetrics);
        }

        /// <summary>
        /// Retrieves a specific sensor metric by its unique ID.
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'AllRoles' policy.
        /// </remarks>
        /// <param name="deviceId">The ID of the device (from URL path).</param>
        /// <param name="groupId">The ID of the group (from URL path).</param>
        /// <param name="sensorMetricId">The unique ID of the sensor metric to retrieve (from URL path).</param>
        /// <returns>Returns 200 OK with the SensorMetric object.</returns>
        [Authorize(Policy = "AllRoles")]
        [HttpGet("{sensorMetricId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SensorMetricResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetSensorMetricById(int deviceId, int groupId, int sensorMetricId)
        {
            var sensorMetric = await _sensorMetricManager.GetSensorMetricByIdAsync(sensorMetricId);
            return Ok(sensorMetric);
        }
    }
}
