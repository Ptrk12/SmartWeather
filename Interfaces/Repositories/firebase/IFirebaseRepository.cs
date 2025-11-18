using Models.firebase;
using Models.responses;

namespace Interfaces.Repositories.firebase
{
    public interface IFirebaseRepository
    {
        Task<IEnumerable<FirebaseDeviceMeasurement>> GetDeviceMeasurementAsync(string deviceSerialNumber);
        Task<FirebaseDeviceMeasurement?> GetLatestDeviceMeasurementAsync(string deviceSerialNumber);
    }
}
