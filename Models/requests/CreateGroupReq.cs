using System.ComponentModel.DataAnnotations;

namespace Models.requests
{
    public class CreateGroupReq
    {
        [Required]
        public string Name { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
    }
}
