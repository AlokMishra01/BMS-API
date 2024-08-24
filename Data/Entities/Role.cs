namespace BMS_API.Data.Entities
{
    public class Role
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string RoleName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserRole> UserRoles { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; }
    }

}
