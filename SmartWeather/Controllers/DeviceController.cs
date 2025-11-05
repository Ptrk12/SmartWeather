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
            var allowedMimeTypes = new[] { "image/jpeg", "image/png" };

            if (req.ImageFile != null && !allowedMimeTypes.Contains(req.ImageFile.ContentType))
            {
                return BadRequest("Invalid image file type. Only JPEG and PNG are allowed.");
            }

            var result = await _deviceManager.AddDeviceAsync(req,groupId);

            return result == true ? Created() : Conflict();
        }
    }
}
