using System.ComponentModel.DataAnnotations;

public class LoginDto
{
    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Username { get; set; }

    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public string Password { get; set; } = default!;
}