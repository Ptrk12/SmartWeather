using Interfaces.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.requests;

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

        [Authorize(Policy = "Admin")]
        [HttpPost("add")]
        public async Task<IActionResult> AddGroup([FromForm]CreateDeviceReq req,int groupId)
        {
            var result = await _deviceManager.AddDeviceAsync(req,groupId);

            return result.Success == true ? Created() : Conflict(result.Message);
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("{deviceId}/update")]
        public async Task<IActionResult> UpdateGroup([FromForm] CreateDeviceReq req,int deviceId, int groupId)
        {        
            var result = await _deviceManager.EditDeviceAsync(req, deviceId,groupId);

            return result.Success == true ? Created() : Conflict(result.Message);
        }
    }
}
