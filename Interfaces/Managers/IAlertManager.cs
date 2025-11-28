using Models.requests;
using Models.responses;

namespace Interfaces.Managers
{
    public interface IAlertManager
    {
        Task<ExecutionResult> CreateAlertRule(CreateAlertReq req, int sensorMetricId, int groupId);
        Task<ExecutionResult> EditAlertRule(CreateAlertReq req, int sensorMetricId, int alertId, int groupId);
        Task<ExecutionResult> DeleteAlertRule(int alertId, int sensorMetricId, int groupId);
        Task<ExecutionResult> GetAlertRuleById(int sensorMetricId, int alertId);
        Task<IEnumerable<AlertRuleResponse>> GetDeviceAlertRules(int deviceId);
    }
}
