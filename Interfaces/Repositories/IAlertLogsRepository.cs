using Models.responses;
using Models.SqlEntities;

namespace Interfaces.Repositories
{
    public interface IAlertLogsRepository
    {
        Task<PagedResult<AlertLog>> GetDeviceAlertLogs(int deviceId, int pageNumber, int pageSize);
        Task InsertAlertLogs(IEnumerable<AlertLog> alertLogs);
    }
}
