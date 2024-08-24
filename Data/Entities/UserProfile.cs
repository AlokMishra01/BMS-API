namespace BMS_API.Data.Entities
{
    public class UserProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }
        public User User { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string Bio { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public Guid PermanentAddressId { get; set; }
        public Guid TemporaryAddressId { get; set; }

        // Navigation Properties
        public Address PermanentAddress { get; set; }
        public Address TemporaryAddress { get; set; }
    }

}
