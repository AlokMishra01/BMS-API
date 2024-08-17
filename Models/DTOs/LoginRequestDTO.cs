using System.ComponentModel.DataAnnotations;

namespace BMS_API.Models.DTOs
{
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "Username or Email is required")]
        public required string UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [CustomPasswordValidation(ErrorMessage = "Password does not meet the required criteria.")]
        public required string Password { get; set; }
    }
}
