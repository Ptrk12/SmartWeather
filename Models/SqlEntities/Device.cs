
using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models.SqlEntities
{
    public class Device
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string SerialNumber { get; set; }
        public DateTimeOffset? LastMeasurement { get; set; }
        public string? Image { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        public DeviceStatus Status { get; set; } = DeviceStatus.Pending;
        [Required]
        public int GroupId { get; set; }
        public virtual Group Group { get; set; }
        public virtual ICollection<SensorMetric> Metrics { get; set; } = new List<SensorMetric>();
    }
}
