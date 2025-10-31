using Interfaces.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.requests;

namespace SmartWeather.Controllers
{
    [Route("api/group")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IGroupManager _groupManager;
        private readonly IUserManager _userManager;

        public GroupController(IGroupManager groupManager, IUserManager userManager)
        {
            _groupManager = groupManager;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddGroup(CreateGroupReq req)
        {
            var userId = _userManager.GetUserId(User);

            var result = await _groupManager.AddGroupAsync(req, userId);

            return result == true ? Created() : Conflict();
        }
    }
}
