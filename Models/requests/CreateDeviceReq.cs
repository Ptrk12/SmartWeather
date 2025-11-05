using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Models.requests
{
    public class CreateDeviceReq
    {
        [Required]
        [StringLength(100)]
        public string SerialNumber { get; set; }
        [Required]
        public string Location { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
