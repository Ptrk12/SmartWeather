
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
    }
}
