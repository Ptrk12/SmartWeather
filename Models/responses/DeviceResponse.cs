namespace Models.responses
{
    public class DeviceResponse
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public string? Image { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public DateTimeOffset? LastMeasurement { get; set; }
        public IEnumerable<AlertStatusResponse> AlertStatuses { get; set; }

    }

    public class AlertStatusResponse
    {
        public bool IsAlert { get; set; }
        public string? AlertMessage { get; set; }
        public string? SensorType{ get; set; }
    }
}
