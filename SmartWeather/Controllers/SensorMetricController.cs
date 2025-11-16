using Interfaces.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.requests;

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

        [Authorize(Policy = "Admin")]
        [HttpPost("add")]
        public async Task<IActionResult> AddSensorMetric(CreateSensorMetric req, int deviceId)
        {
            var result = await _sensorMetricManager.AddSensorMetricAsync(req, deviceId);
            return result.Success == true ? Created() : Conflict(result.Message);
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("/{sensorMetricId}/update")]
        public async Task<IActionResult> UpdateSensorMetric(CreateSensorMetric req, int deviceId, int sensorMetricId, int groupId)
        {
            var result = await _sensorMetricManager.UpdateSensorMetricAsync(deviceId,sensorMetricId,req);
            return result.Success == true ? NoContent() : Conflict(result.Message);
        }

        [Authorize(Policy = "Admin")]
        [HttpDelete("/{sensorMetricId}/delete")]
        public async Task<IActionResult> DeleteSenroMetric(int deviceId, int sensorMetricId, int groupId)
        {
            var result = await _sensorMetricManager.DeleteSensorMetricAsync(deviceId,sensorMetricId);
            return result.Success == true ? NoContent() : Conflict(result.Message);
        }
    }
}
