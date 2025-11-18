
using Models.requests;
using Models.responses;

namespace Interfaces.Managers
{
    public interface ISensorMetricManager
    {
        Task<ExecutionResult> AddSensorMetricAsync(CreateSensorMetric req, int deviceId);
        Task<ExecutionResult> UpdateSensorMetricAsync(int deviceId, int sensorMetricId, CreateSensorMetric req);
        Task<ExecutionResult> DeleteSensorMetricAsync(int deviceId, int sensorMetricId);
        Task<IEnumerable<SensorMetricResponse>> GetSensorMetricsAsync(int deviceId);
    }
}
