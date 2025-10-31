using System.ComponentModel.DataAnnotations;

namespace Models.SqlEntities
{
    public class AlertLog
    {
        public int Id { get; set; }
        public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;
        public double TriggeredValue { get; set; }
        public double TriggeredValueThreshold { get; set; }
        [Required]
        public int AlertId { get; set; }
        public virtual Alert Alert { get; set; }

    }
}
