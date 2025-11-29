
using Models.SqlEntities;

namespace Interfaces.Repositories
{
    public interface IDeviceRepository
    {
        Task<IEnumerable<Device>> GetDevicesInGroupAsync(int groupId);
        Task<string?> GetDeviceSerialNumberAsync(int deviceId);
        Task<Device?> GetLatestHistoricalRecord(string serialNumber);
        Task<bool> IsDeviceAllowedForUser(string userId, int groupId, int deviceId);
        Task UpdateDeviceLastMeasurement(string serialNumber, DateTimeOffset date);
        Task<int?> GetDeviceIdBySerialNumber(string serialNumber);
    }
}
