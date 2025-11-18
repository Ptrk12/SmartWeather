using Models.SqlEntities;

namespace Interfaces.Repositories
{
    public interface ISensorMetricRepository
    {
        Task<bool> IsSensorMetricAllowedForUser(int deviceId, int sensorMetricId);
        Task<IEnumerable<SensorMetric>> GetAllSensorMetricAsync(int deviceId);
    }
}
