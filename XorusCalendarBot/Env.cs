namespace XorusCalendarBot;

public class Env
{
    public string DiscordBotToken { get; }
    public string? DatabasePath { get; }
    public string Secret { get; set; }
    public string ListenUrl { get; set; }

    public Env()
    {
        DiscordBotToken = Environment.GetEnvironmentVariable("BOT_TOKEN") ?? "";
        DatabasePath = Environment.GetEnvironmentVariable("DB_PATH") ?? "database.db";
        Secret = Environment.GetEnvironmentVariable("SECRET") ?? "P8O9HsrlZzCnYaLIMXdlw6PYOlOE8UtGiyTfrbXZ";
        ListenUrl = Environment.GetEnvironmentVariable("LISTEN_URL") ?? "localhost:9876";
    }
}