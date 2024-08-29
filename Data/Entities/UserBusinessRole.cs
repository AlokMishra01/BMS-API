namespace BMS_API.Data.Entities
{
    public class UserBusinessRole
    {
        public Guid Id { get; set; }

        // Foreign keys
        public string UserId { get; set; }
        public Guid BusinessId { get; set; }

        // Using the enum type for Role
        public BusinessRole Role { get; set; }

        // Navigation properties
        public SystemUser User { get; set; }
        public Business Business { get; set; }
    }
}
