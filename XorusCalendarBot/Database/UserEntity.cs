namespace XorusCalendarBot.Database;

public class UserEntity
{
    public Guid Id { get; set; }
    public string[] Guilds { get; set; } = Array.Empty<string>();
    public string DiscordId { get; set; } = null!;
    public string? DiscordName { get; set; }
    public string? DiscordAvatar { get; set; }
    public bool IsAdmin { get; set; } = false;
    public string[] EditGuilds { get; set; } = Array.Empty<string>();
}