namespace BMS_API.Data.Entities
{
    public class UserRole
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }
        public User User { get; set; }
        public Guid RoleId { get; set; }
        public Role Role { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }

}
