namespace XorusCalendarBot.Module.Soundboard.Entity;

public class SoundEntity
{
    public Guid Id { get; set; }
    public string? GuildId { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Uri { get; set; }
    public float? Cooldown { get; set; }
    public DateTime CreatedAt { get; } = DateTime.Now;
    public DateTime? LastUsedAt { get; set; }
    public bool Enabled { get; set; } = true;
    public float? StartSeconds { get; set; }
    public float? EndSeconds { get; set; }

    public string? GetSlug()
    {
        return Slug ?? Name;
    }
}