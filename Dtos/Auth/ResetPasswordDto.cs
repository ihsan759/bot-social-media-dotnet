using System.ComponentModel.DataAnnotations;

public class ResetPasswordDto
{
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = default!;
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = default!;
    [Required(ErrorMessage = "New password is required")]
    public string NewPassword { get; set; } = default!;
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string? ConfirmPassword { get; set; } = default!;
}
