namespace BMS_API.Models.DTOs
{
    public class LoginResponseDTO
    {
        public required string Token { get; set; }
        public required string RefreshToken { get; set; }
    }
}
