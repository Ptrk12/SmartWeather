
using Models.SqlEntities;

namespace Interfaces.Repositories
{
    public interface IGroupRepository : IGenericCrudRepository<Group>
    {
        Task<string> GetUserRoleInGroup(string userId, int groupId);
        Task<IEnumerable<Group>> GetCurrentLoggedUserGroups(string userId);
    }
}
