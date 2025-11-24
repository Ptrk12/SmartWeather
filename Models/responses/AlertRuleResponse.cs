namespace Models.responses
{
    public class AlertRuleResponse
    {
        public int Id { get; set; }
        public string Condition { get; set; }
        public double ThresholdValue { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public int SensorMetricId { get; set; }
    }
}
