using System.ComponentModel.DataAnnotations;

namespace BMS_API.Models.DTOs
{
    public class RefreshTokenDTO
    {
        [Required]
        public string Token { get; set; }
    }
}
