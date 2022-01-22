namespace XorusCalendarBot;

public class Env
{
    public string DiscordBotToken { get; }

    public string? DatabasePath { get; }

    public Env()
    {
        DiscordBotToken = Environment.GetEnvironmentVariable("BOT_TOKEN") ?? "";
        DatabasePath = Environment.GetEnvironmentVariable("DB_PATH");
    }
}