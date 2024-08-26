using BMS_API.Data.Entities;

namespace BMS_API.Models.DTOs
{
    public class UserRoleDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public BusinessRole Role { get; set; }
    }
}
