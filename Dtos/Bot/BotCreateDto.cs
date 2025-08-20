using System.ComponentModel.DataAnnotations;
using BotSocialMedia.Models;

namespace BotSocialMedia.Dtos
{
    public class BotCreateDto
    {
        public IFormFile? Avatar { get; set; } = null;

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string Name { get; set; } = default!;

        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = default!;

        public DateTime? ExpiresAt { get; set; }

        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^(?:\+62|0)?[-. ]?\(?([0-9]{2,4})\)?[-. ]?([0-9]{3,4})[-. ]?([0-9]{4,6})$", ErrorMessage = "Must be a valid phone number")]
        public string Phone { get; set; } = default!;
    }
}
