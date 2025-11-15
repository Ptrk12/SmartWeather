
using Models.requests;
using Models.responses;

namespace Interfaces.Managers
{
    public interface ISensorMetricManager
    {
        Task<ExecutionResult> AddSensorMetricAsync(CreateSensorMetric req, int deviceId);
    }
}
