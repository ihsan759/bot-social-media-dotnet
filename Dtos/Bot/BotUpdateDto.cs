using System.ComponentModel.DataAnnotations;
using BotSocialMedia.Models;

namespace BotSocialMedia.Dtos
{
    public class BotUpdateDto
    {
        public IFormFile? Avatar { get; set; } = null;
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string? Name { get; set; }
        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }
        [Phone(ErrorMessage = "Phone must be a valid phone number.")]
        public string? Phone { get; set; }
        public bool? Active { get; set; }
        public BotStatus? Status { get; set; }
    }
}
