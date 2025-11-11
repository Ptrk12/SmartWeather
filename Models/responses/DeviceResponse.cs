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

        //later add sensorMetrics
        }
}
