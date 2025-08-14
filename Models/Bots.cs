namespace BotSocialMedia.Models;

public class Bots
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public string? AvatarUrl { get; set; } = "";
    public string Name { get; set; } = default!;
    public Platform Platform { get; set; } = Platform.telegram;
    public bool Active { get; set; } = true;
    public string Token { get; set; } = "";
    public DateTime? ExpiresAt { get; set; }
    public string Phone { get; set; } = default!;
    public BotStatus Status { get; set; } = BotStatus.active;
    public DateTime? LastActivityAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public Accounts Account { get; set; } = default!;
    public ICollection<Customs>? Customs { get; set; }
}
