using Models.requests;

namespace Interfaces.Managers
{
    public interface IGroupManager
    {
        Task<bool> AddGroupAsync(CreateGroupReq req, string userId);
    }
}
