using System.ComponentModel.DataAnnotations;

namespace BMS_API.Models.DTOs
{
    public class UserProfileDto
    {
        [Required]
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        [Required]
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Bio { get; set; }
    }
}
