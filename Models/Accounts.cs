using System.ComponentModel.DataAnnotations;

namespace BotSocialMedia.Models;

public class Accounts
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? AvatarUrl { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string Name { get; set; } = default!;
    public DateOnly BirthDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public string Password { get; set; } = default!;
    public Role Role { get; set; } = Role.USER;
    public bool IsVerified { get; set; } = false;
    public DateTime? LastLogin { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public string? VerificationToken { get; set; }
    public DateTime? VerificationTokenExpiry { get; set; }
    public string? ResetPasswordToken { get; set; }
    public DateTime? ResetPasswordTokenExpiry { get; set; }
    public Status Status { get; set; } = Status.active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public ICollection<Bots>? Bots { get; set; }
}
