using Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Models.responses;
using Models.SqlEntities;
using Repositories.SqlContext;

namespace Repositories
{
    public class SensorMetricRepository : GenericCrudRepository<SensorMetric>, ISensorMetricRepository
    {
        private readonly SqlDbContext _context;

        public SensorMetricRepository(SqlDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> IsSensorMetricAllowedForUser(int deviceId, int sensorMetricId)
        {
            try
            {
                var sensorMetric = await _context.SensorMetrics.AsNoTracking().FirstOrDefaultAsync(sm => sm.Id == sensorMetricId && sm.DeviceId == deviceId);

                return sensorMetric != null;
            }
            catch
            {
                return false;
            }
        }
        public async Task<IEnumerable<SensorMetric>> GetAllSensorMetricAsync(int deviceId)
        {
            try
            {
                var result = await _context.SensorMetrics.AsNoTracking().Where(x => x.DeviceId == deviceId).ToListAsync();
                return result;
            }
            catch
            {
                return Enumerable.Empty<SensorMetric>();
            }
        }
    }
}
