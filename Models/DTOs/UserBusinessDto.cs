using BMS_API.Data.Entities;

namespace BMS_API.Models.DTOs
{
    public class UserBusinessDto
    {
        public Guid BusinessId { get; set; }
        public string BusinessName { get; set; }
        public BusinessRole Role { get; set; }
    }
}
