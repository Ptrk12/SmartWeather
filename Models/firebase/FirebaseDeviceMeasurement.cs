using Google.Cloud.Firestore;

namespace Models.firebase
{
    public class FirebaseDeviceMeasurement
    {
        [FirestoreDocumentId]
        public string Id { get; set; }
        [FirestoreProperty("serialNumber")]
        public string SerialNumber { get; set; }
        [FirestoreProperty("timestamp")]
        public long Timestamp { get; set; }
        [FirestoreProperty("parameters")]
        public Dictionary<string, string> Parameters { get; set; }
    }
}
