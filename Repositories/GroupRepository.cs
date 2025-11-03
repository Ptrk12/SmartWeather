using Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Models.responses;
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
        public async Task<bool> DeleteGroupAsync(Group group)
        {
            try
            {      
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Group?> GetGroupByIdAsync(int groupId)
        {
            return await _context.Groups
                .Include(g => g.Devices)
                .FirstOrDefaultAsync(g => g.Id == groupId);
        }

        public async Task<bool> UpdateGroupAsync(Group group) 
        {              
            try
            {
                _context.Groups.Update(group);
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
