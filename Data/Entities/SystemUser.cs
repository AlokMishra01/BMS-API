using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BMS_API.Data.Entities
{
    public class SystemUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        [Required]
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
    }
}
