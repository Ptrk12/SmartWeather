namespace Models.responses
{
    public class MeasurementResponse
    {
        public string Parameter { get; set; }
        public Dictionary<DateTimeOffset, double> Measurements { get; set; } = new();
    }
}
