namespace XorusCalendarBot.Module.Soundboard.Entity;

public class SoundEntity
{
    public Guid Id { get; set; }
    public string GuildId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Uri { get; set; }
    public float? Cooldown { get; set; }
    public DateTime CreatedAt { get; } = DateTime.Now;
    public DateTime? LastUsedAt { get; set; }
    public bool Enabled { get; set; } = true;
    public float? StartSeconds { get; set; }
    public float? EndSeconds { get; set; }
}