using Models.requests;

namespace Interfaces.Managers
{
    public interface IDeviceManager
    {
        Task<bool> AddDeviceAsync(CreateDeviceReq req, int groupId);
    }    
}
