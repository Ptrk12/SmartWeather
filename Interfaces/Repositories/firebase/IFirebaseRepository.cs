using Models.firebase;
using Models.mqtt;
using Models.responses;

namespace Interfaces.Repositories.firebase
{
    public interface IFirebaseRepository
    {
        Task<IEnumerable<FirebaseDeviceMeasurement>> GetDeviceMeasurementAsync(string deviceSerialNumber);
        Task<FirebaseDeviceMeasurement?> GetLatestDeviceMeasurementAsync(string deviceSerialNumber);
        Task PushToFirestore(string serialNumber, MqttMessage message);
    }
}
