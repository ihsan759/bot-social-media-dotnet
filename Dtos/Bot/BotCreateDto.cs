using System.ComponentModel.DataAnnotations;
using BotSocialMedia.Models;

namespace BotSocialMedia.Dtos
{
    public class BotCreateDto
    {
        [Required(ErrorMessage = "AccountId is required.")]
        public Guid AccountId { get; set; }
        public IFormFile? Avatar { get; set; } = null;

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = default!;

        public DateTime? ExpiresAt { get; set; }

        [Required(ErrorMessage = "Phone is required.")]
        [Phone(ErrorMessage = "Phone must be a valid phone number.")]
        public string Phone { get; set; } = default!;
    }
}
