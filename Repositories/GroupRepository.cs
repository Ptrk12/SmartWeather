using Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Models.SqlEntities;
using Repositories.SqlContext;

namespace Repositories
{
    public class GroupRepository : GenericCrudRepository<Group>, IGroupRepository
    {
        private readonly SqlDbContext _context;

        public GroupRepository(SqlDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<string> GetUserRoleInGroup(string userId, int groupId)
        {
            try
            {
                var result = await _context.GroupMemberships
                    .FirstOrDefaultAsync(x => x.ApplicationUserId == userId && x.GroupId == groupId);

                if(result != null)
                {
                  return result.Role.ToString();
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task<IEnumerable<(int Id, string Name)>> GetCurrentLoggedUserGroups(string userId)
        {
            try
            {
                var groups = await _context.GroupMemberships
                    .Where(gm => gm.ApplicationUserId == userId)
                    .Select(gm => new { gm.Group.Id, gm.Group.Name })
                    .AsNoTracking().ToListAsync();

                return groups.Select(g => (g.Id, g.Name));
            }
            catch
            {
                return Enumerable.Empty<(int Id, string Name)>();
            }
        }
    }
}
