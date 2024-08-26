namespace BMS_API.Models.DTOs
{
    public class DepartmentResponseDTO
    {
        public Guid Id { get; set; }
        public required string DepartmentName { get; set; }
        public required string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
