namespace XorusCalendarBot;

public class Env
{
    public Env()
    {
        DiscordBotToken = Environment.GetEnvironmentVariable("BOT_TOKEN") ?? "";
        DatabasePath = Environment.GetEnvironmentVariable("DB_PATH") ?? "database.db";
        Secret = Environment.GetEnvironmentVariable("SECRET") ?? "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcd";
        ListenUrl = Environment.GetEnvironmentVariable("LISTEN_URL") ?? "localhost:9876";
        DiscordClientId = Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID") ?? "";
        DiscordClientSecret = Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET") ?? "";
        DiscordRedirectUri = Environment.GetEnvironmentVariable("DISCORD_REDIRECT_URI") ?? "";
        DiscordAdminId = Environment.GetEnvironmentVariable("DISCORD_ADMIN_ID") ?? "";
        ClientAppHost = Environment.GetEnvironmentVariable("CLIENT_APP_HOST") ?? "";
        StaticHtmlPath = Environment.GetEnvironmentVariable("STATIC_HTML_PATH");
    }

    public string DiscordBotToken { get; }
    public string? DatabasePath { get; }
    public string Secret { get; }
    public string ListenUrl { get; }
    public string DiscordClientId { get; }
    public string DiscordClientSecret { get; }
    public string DiscordRedirectUri { get; }
    public string DiscordAdminId { get; }
    public string ClientAppHost { get; }
    public string? StaticHtmlPath { get; }
}