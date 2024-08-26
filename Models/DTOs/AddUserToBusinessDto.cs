using BMS_API.Data.Entities;

namespace BMS_API.Models.DTOs
{
    public class AddUserToBusinessDto
    {
        public Guid UserId { get; set; }
        public BusinessRole Role { get; set; }
    }
}
