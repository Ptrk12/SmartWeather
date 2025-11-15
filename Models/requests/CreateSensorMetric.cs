using System.ComponentModel.DataAnnotations;

namespace Models.requests
{
    public class CreateSensorMetric
    {
        [Required]
        public string SensorType { get; set; }
        [Required]
        [StringLength(20)]
        public string Unit { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
