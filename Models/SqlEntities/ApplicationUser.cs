
using Microsoft.AspNetCore.Identity;

namespace Models.SqlEntities
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<GroupMembership> GroupMemberships { get; set; } = new List<GroupMembership>();
    }
}
