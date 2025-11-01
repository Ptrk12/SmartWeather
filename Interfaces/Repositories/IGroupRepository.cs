

using Models.SqlEntities;

namespace Interfaces.Repositories
{
    public interface IGroupRepository
    {
        Task<bool> AddGroupAsync(Group req);
        Task<bool> DeleteGroupAsync(Group group);
        Task<Group?> GetGroupByIdAsync(int groupId);
        Task<bool> UpdateGroupAsync(Group group);
        Task<string> GetUserRoleInGroup(string userId, int groupId);
    }
}
