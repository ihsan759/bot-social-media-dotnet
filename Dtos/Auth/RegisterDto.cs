using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Url(ErrorMessage = "Must be a valid URL")]
    public string? AvatarUrl { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Name must contain at least 1 character")]
    public string Name { get; set; } = default!;

    [Required]
    [MinLength(6, ErrorMessage = "Username must contain at least 6 characters")]
    public string Username { get; set; } = default!;

    [Required]
    [EmailAddress(ErrorMessage = "Must be a valid email address")]
    public string Email { get; set; } = default!;

    [Required]
    [RegularExpression(@"^(?:\+62|0)?[-. ]?\(?([0-9]{2,4})\)?[-. ]?([0-9]{3,4})[-. ]?([0-9]{4,6})$", ErrorMessage = "Must be a valid phone number")]
    public string Phone { get; set; } = default!;

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; } = default!;

}
