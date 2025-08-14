namespace BotSocialMedia.Models;

public class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomsId { get; set; }
    public string QuestionText { get; set; } = default!;
    public string Answer { get; set; } = default!;
    public MatchType MatchType { get; set; }
    public bool CaseSensitive { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public Customs Customs { get; set; } = default!;
}
