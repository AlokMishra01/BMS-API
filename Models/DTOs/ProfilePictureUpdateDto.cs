using System.ComponentModel.DataAnnotations;

namespace BMS_API.Models.DTOs
{
    public class ProfilePictureUpdateDto
    {
        [Required]
        public IFormFile ProfilePicture { get; set; }
    }
}
