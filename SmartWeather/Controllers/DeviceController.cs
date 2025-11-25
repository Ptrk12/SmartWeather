using Interfaces.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.requests;
using Models.responses;
using Models.SqlEntities;
using System.Diagnostics.Metrics;

namespace SmartWeather.Controllers
{
    [Route("api/group/{groupId}/device")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceManager _deviceManager;

        public DeviceController(IDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
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
            var result = await _deviceManager.EditDeviceAsync(req, deviceId);

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
        /// Requires authorization with the 'AllRoles' policy. The parameter type is passed via query string.
        /// </remarks>
        /// <param name="groupId">The ID of the group (from URL path).</param>
        /// <param name="deviceId">The ID of the device (from URL path).</param>
        /// <param name="parameterType">The type of measurement parameter to retrieve (from query string) Available values:temperature, humidity, pressure, pm2_5, pm10 </param>
        /// <returns>Returns 200 OK with a list of measurements.</returns>
        [Authorize(Policy = "AllRoles")]
        [HttpGet("{deviceId}/measurements")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MeasurementResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetDeviceMeasurement(int groupId, int deviceId, string parameterType)
        {
            var measurements = await _deviceManager.GetDeviceMeasurementAsync(deviceId, parameterType);
            return Ok(measurements);
        }
    }
}
