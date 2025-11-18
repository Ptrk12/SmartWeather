
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
        public async Task<FirebaseDeviceMeasurement?> GetLatestDeviceMeasurementAsync(string deviceSerialNumber)
        {
            if (string.IsNullOrEmpty(deviceSerialNumber))
                return null;

            var collection = _db.Collection($"{DevicesCollection}/{deviceSerialNumber}/measurements");

            var query = collection
                        .OrderByDescending("timestamp")
                        .Limit(1);

            var snapshot = await query.GetSnapshotAsync();

            var doc = snapshot.Documents.FirstOrDefault();

            if (doc == null || !doc.Exists)
                return null;

            return doc.ConvertTo<FirebaseDeviceMeasurement>();
        }

        public async Task<IEnumerable<FirebaseDeviceMeasurement>> GetDeviceMeasurementAsync(string deviceId)
        {
            var result = new List<FirebaseDeviceMeasurement>();

            if (string.IsNullOrEmpty(deviceId))
                return result;

            var collection = _db.Collection($"{DevicesCollection}/{deviceId}/measurements");

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var thirtyDaysAgo = now - (30 * 24 * 60 * 60);

            var query = collection
                        .WhereGreaterThanOrEqualTo("timestamp", thirtyDaysAgo);

            var snapshot = await query.GetSnapshotAsync();

            if (snapshot.Count == 0)
                return result;

            result = snapshot.Documents
                   .Where(d => d.Exists)
                   .Select(d => d.ConvertTo<FirebaseDeviceMeasurement>())
                   .ToList();

            return result;
        }

    }
}
