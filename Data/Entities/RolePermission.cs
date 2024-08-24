namespace BMS_API.Data.Entities
{
    public class RolePermission
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid RoleId { get; set; }
        public Role Role { get; set; }
        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }

}
