using Models.requests;
using Models.responses;

namespace Interfaces.Managers
{
    public interface IGroupManager
    {
        Task<bool> AddGroupAsync(CreateGroupReq req, string userId);
        Task<ExecutionResult> DeleteGroupAsync(int groupId);
        Task<GroupResponse?> GetGroupByIdAsync(int groupId);
        Task<bool> UpdateGroupAsync(int groupId, CreateGroupReq req);
    }
}
