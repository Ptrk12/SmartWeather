using System.Text.Json.Serialization;

namespace Models.responses
{
    public class PredictionResponse
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("hours")]
        public int Hours { get; set; }

        [JsonPropertyName("predictions")]
        public List<PredictionPoint> Predictions { get; set; }
    }
    public class PredictionPoint
    {
        [JsonPropertyName("time")]
        public DateTime Time { get; set; }

        [JsonPropertyName("PM25")]
        public double Pm25 { get; set; }

        [JsonPropertyName("PM10")]
        public double Pm10 { get; set; }

        [JsonPropertyName("temp_c")]
        public double TemperatureC { get; set; }

        [JsonPropertyName("humidity")]
        public double Humidity { get; set; }

        [JsonPropertyName("pressure")]
        public double Pressure { get; set; }
    }
}
