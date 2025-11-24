using System.ComponentModel.DataAnnotations;

namespace Models.requests
{
    public class CreateAlertReq
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public double ThresholdValue { get; set; }
        [Required]
        public string Condition { get; set; }
        public bool IsEnabled { get; set; }
    }
}
