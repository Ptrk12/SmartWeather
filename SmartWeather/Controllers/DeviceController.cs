using Interfaces.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.requests;
using Models.responses;
using System.Text.Json;

namespace SmartWeather.Controllers
{
    [Route("api/group/{groupId}/device")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceManager _deviceManager;
        private readonly IDeviceMonitorManager _deviceMonitorManager;

        public DeviceController(IDeviceManager deviceManager, IDeviceMonitorManager deviceMonitorManager)
        {
            _deviceManager = deviceManager;
            _deviceMonitorManager = deviceMonitorManager;
        }

        /// <summary>
        /// Adds a new device to the specified group.
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'Admin' policy.
        /// Data is submitted using form data (multipart/form-data) due to the [FromForm] attribute.
        /// The API returns the error message string directly upon a 409 conflict.
        /// </remarks>
        /// <param name="req">The data for the new device (from form data).</param>
        /// <param name="groupId">The ID of the group to which the device is added.</param>
        /// <returns>Returns 201 Created on success, or 409 Conflict on  failure.</returns>
        [Authorize(Policy = "Admin")]
        [HttpPost("add")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddDevice([FromForm]CreateDeviceReq req,int groupId)
        {
            var result = await _deviceManager.AddDeviceAsync(req,groupId);

            return result.Success == true ? Created() : Conflict(result.Message);
        }

        /// <summary>
        /// Updates an existing device within the group.
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'Admin' policy.
        /// Data is submitted using form data. Returns 204 NoContent, which often implies the resource was updated successfully 
        /// </remarks>
        /// <param name="req">The updated device data (from form data).</param>
        /// <param name="deviceId">The ID of the device to update (from URL path).</param>
        /// <param name="groupId">The ID of the group (from URL path).</param>
        /// <returns>Returns 204  on success, or 409 Conflict on failure.</returns>
        [Authorize(Policy = "Admin")]
        [HttpPut("{deviceId}/update")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateDevice([FromForm] CreateDeviceReq req,int deviceId, int groupId)
        {        
            var result = await _deviceManager.EditDeviceAsync(req, deviceId,groupId);

            return result.Success == true ? NoContent() : Conflict(result.Message);
        }

        /// <summary>
        /// Retrieves a list of all devices belonging to a specific group.
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'AllRoles' policy. Returns an empty list [] if the group has no devices.
        /// </remarks>
        /// <param name="groupId">The ID of the group to fetch devices from (from URL path).</param>
        /// <returns>Returns 200 OK with a list of devices.</returns>
        [Authorize(Policy = "AllRoles")]
        [HttpGet("get-devices-in-group")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DeviceResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetDevicesInGroup(int groupId)
        {
            var devices = await _deviceManager.GetDevicesAsync(groupId);
            return Ok(devices);
        }
        /// <summary>
        /// Deletes a device with the specified ID.
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'Admin' policy. Returns 409 if the deletion fails.
        /// </remarks>
        /// <param name="deviceId">The ID of the device to delete (from URL path).</param>
        /// <param name="groupId">The ID of the group (from URL path).</param>
        /// <returns>Returns 204 NoContent on success, or 409 on failure.</returns>
        [Authorize(Policy = "Admin")]
        [HttpDelete("{deviceId}/delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteDevice(int deviceId, int groupId)
        {
            var result = await _deviceManager.DeleteDeviceAsync(deviceId);
            if (result == false)
            {
                return Conflict();
            }
            return NoContent();
        }

        /// <summary>
        /// Retrieves measurement data for a specific device and parameter type.
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'AllRoles' policy.
        /// 
        /// **Date Range Constraint:**
        /// The maximum difference between <paramref name="dateTo"/> and <paramref name="dateFrom"/> 
        /// **must not exceed 30 days**.
        /// </remarks>
        /// <param name="groupId">The ID of the group (from URL path).</param>
        /// <param name="deviceId">The ID of the device (from URL path).</param>
        /// <param name="parameterType">The type of measurement parameter to retrieve (from query string). Available values: temperature, humidity, pressure, pm2_5, pm10 </param>
        /// <param name="dateFrom">Optional start date to filter measurements. **(Max 30 days prior to dateTo)**.</param>
        /// <param name="dateTo">Optional end date to filter measurements (defaults to current time).</param>
        /// <returns>Returns 200 OK with measurements </returns>
        [Authorize(Policy = "AllRoles")]
        [HttpGet("{deviceId}/measurements")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MeasurementResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetDeviceMeasurement(int groupId, int deviceId, string parameterType, DateTimeOffset? dateFrom, DateTimeOffset? dateTo)
        {
            var measurements = await _deviceManager.GetDeviceMeasurementAsync(deviceId, parameterType,dateFrom,dateTo);
            return Ok(measurements);
        }

        [Authorize(Policy = "AllRoles")]
        [HttpGet("{deviceId}/predict-measurements")]
        public async Task<IActionResult> PredictWeatherParameters(int deviceId, string parameterType, int hours, string model)
        {
            var result = await _deviceManager.PredictWeatherParameters(deviceId, parameterType, hours, model);
            return result.Success == true ? Ok(result.Data) : Conflict(result.Message);
        }

        /// <summary>
        /// Establishes a real-time event stream to monitor alerts for a specific device.
        /// </summary>
        /// <remarks>
        /// Requires authorization with the 'AllRoles' policy.
        /// 
        /// **Protocol Information:**
        /// This endpoint uses **Server-Sent Events (SSE). It keeps the connection open and pushes data 
        /// to the client whenever an alert status changes (new alert or alert resolved).
        /// 
        /// **Data Format:**
        /// The stream sends data in `text/event-stream` format. Each message is a JSON object 
        /// prefixed with `data: `.
        /// 
        /// **Connection Management:**
        /// - The server sends update every 1 minute.
        /// - The connection remains active until the client disconnects or the server stops.
        /// - Proxy servers (like Nginx) are instructed not to buffer the response (`X-Accel-Buffering: no`).
        /// </remarks>
        /// <param name="deviceId">The ID of the device to monitor (from URL path).</param>
        /// <returns>
        /// Returns a continuous stream of <see cref="AlertStreamResultResponse"/> objects.
        /// </returns>
        [Authorize(Policy = "AllRoles")]
        [HttpGet("{deviceId}/alerts/stream")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AlertStreamResultResponse))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task GetAlertsStream(int deviceId)
        {
            IHeaderDictionary headers = Response.Headers;

            headers.Append("Content-Type", "text/event-stream");
            headers.Append("Cache-Control", "no-cache");
            headers.Append("Connection", "keep-alive");
            headers.Append("X-Accel-Buffering", "no");

            var cancellationToken = HttpContext.RequestAborted;

            try
            {
                await foreach(var alerts in _deviceMonitorManager.MonitorDeviceStream(deviceId, cancellationToken))
                {
                    var json = JsonSerializer.Serialize(alerts);
                    await Response.WriteAsync($"data: {json}\n\n");
                    await Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException) { }
        }
    }
}
