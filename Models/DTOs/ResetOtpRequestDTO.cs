using System.ComponentModel.DataAnnotations;

namespace BMS_API.Models.DTOs
{
    public class ResetOtpRequestDTO
    {
        [Required]
        [Range(100000, 999999, ErrorMessage = "Invalid OTP")]
        public string Otp { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [CustomPasswordValidation(ErrorMessage = "New password does not meet the required criteria.")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
