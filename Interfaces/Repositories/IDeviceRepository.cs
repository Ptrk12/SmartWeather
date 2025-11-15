
using Models.SqlEntities;

namespace Interfaces.Repositories
{
    public interface IDeviceRepository
    {
        Task<IEnumerable<Device>> GetDevicesInGroupAsync(int groupId);
        Task<string?> GetDeviceSerialNumberAsync(int deviceId);
    }
}
