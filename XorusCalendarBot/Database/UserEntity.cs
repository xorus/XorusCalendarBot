using LiteDB;

namespace XorusCalendarBot.Database;

public class UserEntity
{
    public Guid Id { get; set; }
    public ulong[] Guilds { get; set; }
    public string Name { get; set; }
    public string Key { get; set; }
    public ulong DiscordId { get; set; }
}