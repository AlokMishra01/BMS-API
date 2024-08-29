using Microsoft.AspNetCore.Identity;

namespace BMS_API.Data.Entities
{
    public class SystemUser : IdentityUser
    {
        public UserProfile UserProfile { get; set; }
        public ICollection<UserBusinessRole> UserBusinessRoles { get; set; }
        public Guid? ActiveBusinessId { get; set; }
    }
}
