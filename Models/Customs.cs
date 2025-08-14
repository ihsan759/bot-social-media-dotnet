namespace BotSocialMedia.Models;

public class Customs
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BotsId { get; set; }
    public string Name { get; set; } = default!;
    public int Delay { get; set; } = 5;
    public bool Enabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public Bots Bots { get; set; } = default!;
    public ICollection<Question>? Questions { get; set; }
}
