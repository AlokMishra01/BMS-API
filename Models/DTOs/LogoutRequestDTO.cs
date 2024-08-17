using System.ComponentModel.DataAnnotations;

namespace BMS_API.Models.DTOs
{
    public class LogoutRequestDTO
    {
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
