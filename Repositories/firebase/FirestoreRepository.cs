
using Google.Cloud.Firestore;
using Interfaces.Repositories.firebase;
using Models.firebase;
using Models.responses;
namespace Repositories.firebase
{
    public class FirebaseRepository : IFirebaseRepository
    {
        private readonly FirestoreDb _db;
        private const string DevicesCollection = "devices";
        public FirebaseRepository(FirestoreDb db)
        {
            _db = db;   
        }
        public async Task<ExecutionResult> GetDeviceMeasurementAsync(string deviceId)
        {
            var result = new ExecutionResult();

            if (string.IsNullOrEmpty(deviceId))
                return result;

            var collection = _db.Collection($"{DevicesCollection}/{deviceId}/measurements");

            var snapshot = await collection.GetSnapshotAsync();
            result.Success = true;

            if (snapshot.Count == 0)
                return result;

            result.Data = snapshot.Documents
                           .Where(d => d.Exists)
                           .Select(d => d.ConvertTo<FirebaseDeviceMeasurement>())
                           .ToList();
            return result;
        }

    }
}
