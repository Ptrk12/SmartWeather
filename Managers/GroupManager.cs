using Interfaces.Managers;
using Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Models.requests;
using Models.responses;
using Models.SqlEntities;

namespace Managers
{
    public class GroupManager : IGroupManager
    {
        private readonly IGroupRepository _groupRepository;
        public GroupManager(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        public async Task<bool> AddGroupAsync(CreateGroupReq req, string userId)
        {
            var group = new Group
            {
                Name = req.Name,
                Description = req.Description,
            };

            var membership = new GroupMembership
            {
                ApplicationUserId = userId,
                Group = group,
                Role = Core.Enums.GroupRole.Admin
            };
            group.Memberships.Add(membership);

            return await _groupRepository.AddAsync(group);
        }

        public async Task<IEnumerable<GroupResponse>> GetAllGroupsAsync(string userId)
        {
            var groups = await _groupRepository.GetCurrentLoggedUserGroups(userId);
            return groups.Select(group => new GroupResponse
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                CreatedAt = group.CreatedAt,
                NumberOfDevices = group.Devices.Count
            });
        }
        public async Task<ExecutionResult> DeleteGroupAsync(int groupId)
        {
            var result = new ExecutionResult();

            var group = await _groupRepository.GetByIdAsync(groupId,x=>x.Include(x=>x.Devices));

            if (group == null)
                return result;

            if(group.Devices.Any())
            {
                result.Message = "Group has associated devices and cannot be deleted";
                return result;
            }

            result.Success = await _groupRepository.DeleteAsync(group);
            return result;
        }

        public async Task<GroupResponse?> GetGroupByIdAsync(int groupId)
        {
            var group = await _groupRepository.GetByIdAsync(groupId);

            if (group == null)
                return null;

            return new GroupResponse
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                CreatedAt = group.CreatedAt,
                NumberOfDevices = group.Devices.Count
            };
        }

        public async Task<bool> UpdateGroupAsync(int groupId, CreateGroupReq req)
        {
            var group = await _groupRepository.GetByIdAsync(groupId);
            if (group == null)
                return false;

            group.Name = req.Name;
            group.Description = req.Description;
            return await _groupRepository.UpdateAsync(group);
        }
    }
}
