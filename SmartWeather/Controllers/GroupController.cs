using Interfaces.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.requests;
using Models.responses;

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

        /// <summary>
        /// Creates a new group based on the provided request data.
        /// </summary>
        /// <remarks>
        /// This operation requires the user to be authorized.
        /// 
        /// **The 409 Conflict code** is returned if a group creation failed.
        /// </remarks>
        /// <param name="req">The object containing the data needed to create the group.</param>
        /// <returns>Returns 201 Created on success, or 409 Conflict on business failure.</returns>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddGroup(CreateGroupReq req)
        {
            var userId = _userManager.GetUserId(User);

            var result = await _groupManager.AddGroupAsync(req, userId);

            return result == true ? Created() : Conflict();
        }

        /// <summary>
        /// Deletes a specific group based on its ID.
        /// </summary>
        /// <remarks>
        /// This operation requires a user authorized under the 'Admin' policy.
        /// 
        /// **The 409 Conflict code** is returned when deletion is prevented due to a business rule 
        /// violation (the group has devices). In this case, a descriptive error message (string) 
        /// is returned in the response body.
        /// </remarks>
        /// <param name="groupId">The unique identifier of the group to be deleted.</param>
        /// <returns>Returns 204 NoContent on success, or 404 Not Found, or 409 Conflict on failure.</returns>
        [Authorize(Policy = "Admin")]
        [HttpDelete("{groupId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]

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

        /// <summary>
        /// Retrieves a specific group by its unique ID.
        /// </summary>
        /// <remarks>
        /// This operation requires the user to be authorized under the 'AllRoles' policy.
        /// 
        /// The API returns the full Group object if found.
        /// </remarks>
        /// <param name="groupId">The unique identifier of the group to retrieve.</param>
        /// <returns>Returns 200 OK with the Group object, or 404 Not Found.</returns>
        [Authorize(Policy = "AllRoles")]
        [HttpGet]
        [Route("{groupId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GroupResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetGroupById(int groupId)
        {
            var group = await _groupManager.GetGroupByIdAsync(groupId);
            if (group == null)
                return NotFound();
            return Ok(group);
        }

        /// <summary>
        /// Retrieves a list of all groups associated with the currently authenticated user.
        /// </summary>
        /// <remarks>
        /// This operation requires the user to be authorized under the 'AllRoles' policy.
        /// 
        /// The API uses the authenticated user's ID to fetch the relevant groups.
        /// It returns an empty array [] if the user is not associated with any groups.
        /// </remarks>
        /// <returns>Returns 200 OK with a list of Group objects.</returns>
        [Authorize(Policy = "AllRoles")]
        [HttpGet]
        [Route("get-all")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GroupResponse>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllUserGroups()
        {
            var userId = _userManager.GetUserId(User);
            var group = await _groupManager.GetAllGroupsAsync(userId);
           
            return Ok(group);
        }

        /// <summary>
        /// Updates an existing group based on its ID and new data.
        /// </summary>
        /// <remarks>
        /// This operation requires the user to be authorized under the 'Admin' policy.
        /// 
        /// **The 204 No Content code** is returned if the update is successful.
        /// **The 409 Conflict code** is returned if the update fails
        /// </remarks>
        /// <param name="groupId">The unique identifier of the group to update.</param>
        /// <param name="req">The object containing the new data for the group.</param>
        /// <returns>Returns 204 NoContent on success, or 409 Conflict on failure.</returns>
        [Authorize(Policy = "Admin")]
        [HttpPut("{groupId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateGroup(int groupId, CreateGroupReq req)
        {
            var result = await _groupManager.UpdateGroupAsync(groupId, req);
            return result ? NoContent() : Conflict();
        }

    }
}
