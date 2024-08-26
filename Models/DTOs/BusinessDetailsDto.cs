using BMS_API.Data.Entities;

namespace BMS_API.Models.DTOs
{
    public class BusinessDetailsDto
    {
        public Business Business { get; set; }
        public BusinessRole Role { get; set; }
    }
}
