namespace BotSocialMedia.Models;

public enum Role
{
    USER,
    ADMIN
}

public enum MatchType
{
    exact,
    contains,
    startsWith,
    endsWith
}

public enum Status
{
    active,
    inactive,
    suspended
}

public enum Platform
{
    whatsapp,
    telegram
}

public enum BotStatus
{
    active,
    paused,
    stopped,
    error,
    offline
}
