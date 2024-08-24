namespace BMS_API.Data.Entities
{
    public class UserDepartment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }
        public User User { get; set; }
        public Guid DepartmentId { get; set; }
        public Department Department { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }

}
