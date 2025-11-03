
using Models.SqlEntities;

namespace Interfaces.Repositories
{
    public interface IGroupRepository : IGenericCrudRepository<Group>
    {
        Task<string> GetUserRoleInGroup(string userId, int groupId);
    }
}
