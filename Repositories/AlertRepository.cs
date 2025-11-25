using Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Models.SqlEntities;
using Repositories.SqlContext;

namespace Repositories
{
    public class AlertRepository : IAlertRepository
    {
        private readonly SqlDbContext _db;

        public AlertRepository(SqlDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Alert>> GetDeviceAlertRules(int deviceId)
        {
            try
            {
                return await _db.Alerts.Include(x=>x.SensorMetric).Where(a => a.SensorMetric.DeviceId == deviceId).ToListAsync();
            }
            catch
            {
               return Enumerable.Empty<Alert>();
            }
        }
    }
}
