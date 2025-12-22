using Microsoft.AspNetCore.Http;
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
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
