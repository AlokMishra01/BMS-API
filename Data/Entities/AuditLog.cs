namespace BMS_API.Data.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }
        public User User { get; set; }
        public string Action { get; set; }
        public string ActionDetails { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
