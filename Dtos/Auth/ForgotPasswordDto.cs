using System.ComponentModel.DataAnnotations;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = default!;
}
