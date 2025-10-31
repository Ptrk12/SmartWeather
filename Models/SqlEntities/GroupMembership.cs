using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Models.SqlEntities
{
    public class GroupMembership
    {
        [Required]
        public int GroupId { get; set; }
        public virtual Group Group { get; set; }
        [Required]

        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public GroupRole Role { get; set; } = GroupRole.Member;
    }
}
