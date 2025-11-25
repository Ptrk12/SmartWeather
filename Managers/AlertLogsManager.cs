using Interfaces.Managers;
using Interfaces.Repositories;
using Models.responses;

namespace Managers
{
    public class AlertLogsManager : IAlertLogsManager
    {
        private readonly IAlertLogsRepository _alertLogsRepository;

        public AlertLogsManager(IAlertLogsRepository alertLogsRepository)
        {
            _alertLogsRepository = alertLogsRepository;
        }

        public async Task<PagedResult<AlertLogResponse>> GetDeviceAlertLogs(int deviceId, int pageNumber, int pageSize)
        {
            var alertLogs = await _alertLogsRepository.GetDeviceAlertLogs(deviceId, pageNumber, pageSize);

            var response = alertLogs.Records.Select(al => new AlertLogResponse
            {
                Id = al.Id,
                AlertId = al.AlertId,
                OccuredDate = al.TimeStamp,
                TriggeredValue = al.TriggeredValue,
                TriggeredValueThreshold = al.TriggeredValueThreshold,
                SensorType = al.Alert.SensorMetric.SensorType.ToString()
            });

            return new PagedResult<AlertLogResponse>() { Records = response, TotalRecords = alertLogs.TotalRecords};
        }
    }
}
