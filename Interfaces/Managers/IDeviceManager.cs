using Models.requests;
using Models.responses;

namespace Interfaces.Managers
{
    public interface IDeviceManager
    {
        Task<ExecutionResult> AddDeviceAsync(CreateDeviceReq req, int groupId);
        Task<ExecutionResult> EditDeviceAsync(CreateDeviceReq req, int deviceId, int groupId);
    }    
}
