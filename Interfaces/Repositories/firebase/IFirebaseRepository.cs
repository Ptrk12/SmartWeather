using Models.firebase;
using Models.responses;

namespace Interfaces.Repositories.firebase
{
    public interface IFirebaseRepository
    {
        Task<ExecutionResult> GetDeviceMeasurementAsync(string deviceId);
    }
}
