using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models.SqlEntities
{
    public class SensorMetric
    {
        public int Id { get; set; }
        [Required]
        public SensorType SensorType { get; set; }
        [Required]
        [StringLength(20)]
        public string Unit { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public int DeviceId { get; set; }
        public virtual Device Device { get; set; }
        public virtual Alert Alert { get; set; }
    }
}
