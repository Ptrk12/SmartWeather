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
    }
}
