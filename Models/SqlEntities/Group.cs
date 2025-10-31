using System.ComponentModel.DataAnnotations;

namespace Models.SqlEntities
{
    public class Group
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
        public virtual ICollection<GroupMembership> Memberships { get; set; } = new List<GroupMembership>();
    }
}
