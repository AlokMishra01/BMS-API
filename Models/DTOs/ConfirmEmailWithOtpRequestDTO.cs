using System.ComponentModel.DataAnnotations;

namespace BMS_API.Models.DTOs
{
    public class ConfirmEmailWithOtpRequestDTO
    {
        [Required]
        [Range(100000, 999999, ErrorMessage = "Invalid OTP")]
        public required string Otp { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public required string Email { get; set; }
    }
}
