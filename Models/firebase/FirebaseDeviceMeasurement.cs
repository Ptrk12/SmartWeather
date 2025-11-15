using Google.Cloud.Firestore;

namespace Models.firebase
{
    [FirestoreData]
    public class FirebaseDeviceMeasurement
    {
        public FirebaseDeviceMeasurement()
        {
            
        }
        [FirestoreDocumentId]
        public string Id { get; set; }
        [FirestoreProperty("serialNumber")]
        public string SerialNumber { get; set; }
        [FirestoreProperty("timestamp")]
        public long Timestamp { get; set; }
        [FirestoreProperty("parameters")]
        public List<Dictionary<string, object>> Parameters { get; set; } = new();
    }
}
