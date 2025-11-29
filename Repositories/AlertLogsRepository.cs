using Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Models.responses;
using Models.SqlEntities;
using Repositories.SqlContext;

namespace Repositories
{
    public class AlertLogsRepository : IAlertLogsRepository
    {
        private readonly SqlDbContext _db;

        public AlertLogsRepository(SqlDbContext db)
        {
            _db = db;
        }

        public async Task InsertAlertLogs(IEnumerable<AlertLog> alertLogs)
        {
            try
            {
                await _db.AlertLogs.AddRangeAsync(alertLogs);
                await _db.SaveChangesAsync();
            }
            catch
            {
                // log here
            }
        }
        public async Task<PagedResult<AlertLog>> GetDeviceAlertLogs(int deviceId, int pageNumber, int pageSize)
        {
            try
            {
                var query = _db.AlertLogs
                .Include(x => x.Alert).ThenInclude(x => x.SensorMetric)
                .Where(x => x.Alert.SensorMetric.DeviceId == deviceId);

                var total = await query.CountAsync();

                var items = await query
                    .OrderByDescending(x => x.TimeStamp)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<AlertLog> { Records = items, TotalRecords = total };
            }
            catch
            {
                return new PagedResult<AlertLog> { Records = Enumerable.Empty<AlertLog>(), TotalRecords = 0 };
            }
        }

    }
}
