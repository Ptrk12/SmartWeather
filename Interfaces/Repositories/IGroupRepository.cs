

using Models.SqlEntities;

namespace Interfaces.Repositories
{
    public interface IGroupRepository
    {
        Task<bool> AddGroupAsync(Group req);
    }
}
