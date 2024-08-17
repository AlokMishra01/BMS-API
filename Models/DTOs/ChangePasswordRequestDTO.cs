using System.ComponentModel.DataAnnotations;

namespace BMS_API.Models.DTOs
{
    public class ChangePasswordRequestDTO
    {
        [Required(ErrorMessage = "Current password is required")]
        [CustomPasswordValidation(ErrorMessage = "Current password does not meet the required criteria.")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [CustomPasswordValidation(ErrorMessage = "New password does not meet the required criteria.")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
