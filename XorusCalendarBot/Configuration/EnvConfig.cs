namespace XorusCalendarBot.Configuration;

public class EnvConfig
{
    public string DiscordBotToken { get; }

    public string? DatabasePath { get; }

    public EnvConfig()
    {
        DiscordBotToken = Environment.GetEnvironmentVariable("BOT_TOKEN") ?? "";
        DatabasePath = Environment.GetEnvironmentVariable("DB_PATH");
    }
}