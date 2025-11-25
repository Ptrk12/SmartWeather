using Models.responses;

namespace Interfaces.Managers
{
    public interface IAlertLogsManager
    {
        Task<PagedResult<AlertLogResponse>> GetDeviceAlertLogs(int deviceId, int pageNumber, int pageSize);
    }
}
