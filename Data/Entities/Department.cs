namespace BMS_API.Data.Entities
{
    public class Department
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string DepartmentName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserDepartment> UserDepartments { get; set; }
    }

}
