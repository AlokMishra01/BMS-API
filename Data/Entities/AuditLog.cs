using System.ComponentModel.DataAnnotations;

namespace BMS_API.Data.Entities
{
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserProfileId { get; set; }
        public UserProfile UserProfile { get; set; }

        [Required]
        public string Action { get; set; }
        public string ActionDetails { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
