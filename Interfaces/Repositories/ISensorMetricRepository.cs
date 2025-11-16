namespace Interfaces.Repositories
{
    public interface ISensorMetricRepository
    {
        Task<bool> IsSensorMetricAllowedForUser(int deviceId, int sensorMetricId);
    }
}
