namespace Models.responses
{
    
    public class AlertLogResponse
    {
        public int Id { get; set; }
        public DateTimeOffset OccuredDate { get; set; } = DateTimeOffset.UtcNow;
        public double TriggeredValue { get; set; }
        public double TriggeredValueThreshold { get; set; }
        public int AlertId { get; set; }
        public string SensorType { get; set; }
    }
}
