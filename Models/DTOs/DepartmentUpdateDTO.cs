using System.ComponentModel.DataAnnotations;

namespace BMS_API.Models.DTOs
{
    public class DepartmentUpdateDTO
    {
        [Required]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "DepartmentName is required")]
        [StringLength(50, ErrorMessage = "DepartmentName must be between 3 and 50 characters", MinimumLength = 3)]
        public string DepartmentName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description must be between 3 and 500 characters", MinimumLength = 3)]
        public string Description { get; set; } = string.Empty;
    }
}
