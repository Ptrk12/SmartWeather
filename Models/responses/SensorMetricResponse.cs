using Core.Enums;

namespace Models.responses
{
    public class SensorMetricResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string SensorType { get; set; }
        public double? LatestMeasurement { get; set; }   
    }
}
