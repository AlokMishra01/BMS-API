using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BMS_API.Data.Entities
{
    public class User : IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        [Required]
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<UserDepartment> UserDepartments { get; set; }
        public UserProfile UserProfile { get; set; }
        public ICollection<AuditLog> AuditLogs { get; set; }
    }
}
