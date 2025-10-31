using Interfaces.Managers;
using Interfaces.Repositories;
using Models.requests;
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
            };
            group.Memberships.Add(membership);

            return await _groupRepository.AddGroupAsync(group);
        }
    }
}
