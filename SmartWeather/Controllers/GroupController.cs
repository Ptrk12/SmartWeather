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

        //ADD authorize later now for test isn't needed
        [HttpDelete("delete/{groupId}")]
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            var result = await _groupManager.DeleteGroupAsync(groupId);
            if (!result.Success)
            {
                if (string.IsNullOrEmpty(result.Message))
                    return NotFound();
                else
                    return Conflict(result.Message);
            }
            return NoContent();
        }

        [HttpGet]
        [Route("{groupId}")]
        public async Task<IActionResult> GetGroupById(int groupId)
        {
            var group = await _groupManager.GetGroupByIdAsync(groupId);
            if (group == null)
                return NotFound();
            return Ok(group);
        }

        [HttpPut("{groupId}")]
        public async Task<IActionResult> UpdateGroup(int groupId, CreateGroupReq req)
        {
            var result = await _groupManager.UpdateGroupAsync(groupId, req);
            return result ? NoContent() : Conflict();
        }

    }
}
