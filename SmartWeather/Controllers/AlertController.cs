using Interfaces.Managers;
using Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.requests;

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

        [Authorize(Policy = "Admin")]
        [HttpPost("sensor-metric/{sensorMetricId}/alert-rule/add")]
        public async Task<IActionResult> AddAlertRule(CreateAlertReq req, int groupId, int deviceId, int sensorMetricId)
        {
            var result = await _alertManager.CreateAlertRule(req, sensorMetricId);

            return result.Success == true ? Created() : Conflict(result.Message);
        }
        [Authorize(Policy = "Admin")]
        [HttpPut("sensor-metric/{sensorMetricId}/alert-rule{alertId}/update")]
        public async Task<IActionResult> UpdateAlertRule(CreateAlertReq req, int groupId, int deviceId, int sensorMetricId, int alertId)
        {
            var result = await _alertManager.EditAlertRule(req,sensorMetricId,alertId);

            return result.Success == true ? Created() : Conflict(result.Message);
        }

        [Authorize(Policy = "Admin")]
        [HttpDelete("sensor-metric/{sensorMetricId}/alert-rule{alertId}/delete")]
        public async Task<IActionResult> DeleteAlertRule(int groupId, int deviceId, int sensorMetricId, int alertId)
        {
            var result = await _alertManager.DeleteAlertRule(alertId,sensorMetricId);

            return result.Success == true ? Created() : Conflict(result.Message);
        }
        [Authorize(Policy = "AllRoles")]
        [HttpGet("get-device-alert-rules")]
        public async Task<IActionResult> GetDeviceAlertRules(int groupId, int deviceId)
        {
            var devices = await _alertManager.GetDeviceAlertRules(deviceId);
            return Ok(devices);
        }

        [Authorize(Policy = "AllRoles")]
        [HttpGet("sensor-metric/{sensorMetricId}/alert-rule/{alertId}")]
        public async Task<IActionResult> GetAlertRuleById(int groupId, int deviceId,int sensorMetricId, int alertId)
        {
            var result = await _alertManager.GetAlertRuleById(sensorMetricId,alertId);
            return result.Success == true ? Ok(result.Data) : Conflict(result.Message);
        }
    }
}
