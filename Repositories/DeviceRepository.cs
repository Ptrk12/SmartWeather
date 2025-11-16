
using Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Models.SqlEntities;
using Repositories.SqlContext;

namespace Repositories
{
    public class DeviceRepository : GenericCrudRepository<Device>, IDeviceRepository
    {
        private readonly SqlDbContext _context;

        public DeviceRepository(SqlDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Device>> GetDevicesInGroupAsync(int groupId)
        {
            try
            {
                //add later metrics
                return await _context.Devices.Where(d => d.GroupId == groupId).ToListAsync();
            }
            catch
            {
                return Enumerable.Empty<Device>();
            }
        }
        public async Task<string?> GetDeviceSerialNumberAsync(int deviceId)
        {
            try
            {
                var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);
                return device?.SerialNumber;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Device?> GetLatestHistoricalRecord(string serialNumber)
        {
            try
            {
                var latestHistoryRecord = await _context.Devices
                    .TemporalAll()
                    .Where(d => d.SerialNumber == serialNumber)
                    .Where(d => EF.Property<DateTime>(d, "ValidTo") < DateTime.UtcNow)
                    .OrderByDescending(d => EF.Property<DateTime>(d, "ValidTo"))      
                    .FirstOrDefaultAsync(); 

                return latestHistoryRecord;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> IsDeviceAllowedForUser(string userId, int groupId, int deviceId)
        {
            try
            {
                var device = await _context.Devices.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == deviceId && d.GroupId == groupId);

                if (device == null)
                    return false;

                var membership = await _context.GroupMemberships.AsNoTracking()
                    .FirstOrDefaultAsync(gm => gm.ApplicationUserId == userId && gm.GroupId == groupId);

                return membership != null;
            }
            catch
            {
                return false;
            }
        }

    }
}
