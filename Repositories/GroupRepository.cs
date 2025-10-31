using Interfaces.Repositories;
using Models.SqlEntities;
using Repositories.SqlContext;

namespace Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly SqlDbContext _context;

        public GroupRepository(SqlDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddGroupAsync(Group req)
        {
            try
            {
                _context.Groups.Add(req);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
