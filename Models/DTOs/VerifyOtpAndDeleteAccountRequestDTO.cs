using System.ComponentModel.DataAnnotations;

namespace BMS_API.Models.DTOs
{
    public class VerifyOtpAndDeleteAccountRequestDTO
    {
        [Required]
        [Range(100000, 999999, ErrorMessage = "Invalid OTP")]
        public string Otp { get; set; }
    }
}
