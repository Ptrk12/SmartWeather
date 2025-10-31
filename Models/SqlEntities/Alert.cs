
using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models.SqlEntities
{
    public class Alert
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public AlertCondition Condition { get; set; }
        [Required]
        public double ThresholdValue { get; set; }
        public bool IsEnabled { get; set; } = true;

        [Required]
        public int SensorMetricId { get; set; }
        public virtual SensorMetric SensorMetric { get; set; }
        public virtual ICollection<AlertLog> AlertLogs { get; set; } = new List<AlertLog>();
    }
}
