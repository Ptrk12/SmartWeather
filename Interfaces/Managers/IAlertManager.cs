using Models.requests;
using Models.responses;

namespace Interfaces.Managers
{
    public interface IAlertManager
    {
        Task<ExecutionResult> CreateAlertRule(CreateAlertReq req, int sensorMetricId);
        Task<ExecutionResult> EditAlertRule(CreateAlertReq req, int sensorMetricId, int alertId);
        Task<ExecutionResult> DeleteAlertRule(int alertId, int sensorMetricId);
        Task<ExecutionResult> GetAlertRuleById(int sensorMetricId, int alertId);
        Task<IEnumerable<AlertRuleResponse>> GetDeviceAlertRules(int deviceId);
    }
}
