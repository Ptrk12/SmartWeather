using System.ComponentModel.DataAnnotations;

namespace Models.requests
{
    public class CreateDeviceReq
    {
        [Required]
        [StringLength(100)]
        public string SerialNumber { get; set; }
        [Required]
        public string Location { get; set; } 
        public string? Image { get; set; }
    }
}
