using Models.firebase;
using Models.requests;
using Models.responses;

namespace Interfaces.Managers
{
    public interface IDeviceManager
    {
        Task<ExecutionResult> AddDeviceAsync(CreateDeviceReq req, int groupId);
        Task<ExecutionResult> EditDeviceAsync(CreateDeviceReq req, int deviceId, int groupId);
        Task<IEnumerable<DeviceResponse>> GetDevicesAsync(int groupId);
        Task<bool> DeleteDeviceAsync(int deviceId);
        Task<MeasurementResponse> GetDeviceMeasurementAsync(int deviceId, string parameterType, DateTimeOffset? dateFrom, DateTimeOffset? dateTo);
        Task<IEnumerable<AlertStatusResponse>> GetDeviceAlerts(string deviceSerialNumber, int deviceId);
        Task<ExecutionResult> PredictWeatherParameters(int deviceId, string parameterType, int hours, string model);
    }    
}
