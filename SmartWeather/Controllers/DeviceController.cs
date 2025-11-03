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
        public async Task<IActionResult> AddGroup(CreateDeviceReq req,int groupId)
        {
            var result = await _deviceManager.AddDeviceAsync(req,groupId);

            return result == true ? Created() : Conflict();
        }
    }
}
