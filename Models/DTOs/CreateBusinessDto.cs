namespace BMS_API.Models.DTOs
{
    public class CreateBusinessDto
    {
        public required string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
