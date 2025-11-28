namespace Models.mqtt
{
    public class MqttMessage
    {
        public string SerialNumber { get; set; }
        public long Timestamp { get; set; }
        public List<Dictionary<string, double>> Parameters { get; set; }
    }
}
