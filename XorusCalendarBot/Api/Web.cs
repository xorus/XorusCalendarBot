using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.BearerToken;
using EmbedIO.WebApi;
using Swan.DependencyInjection;
using Swan.Logging;
using XorusCalendarBot.Database;
using XorusCalendarBot.Discord;

namespace XorusCalendarBot.Api;

public class Web : IDisposable
{
    private readonly DatabaseManager _db;
    private readonly DiscordManager _discordManager;
    private readonly WebServer _server;
    private Env _env;

    // public static async Task SerializationCallback(IHttpContext context, object? data)
    // {
    //     Validate.NotNull(nameof(context), context).Response.ContentType = MimeType.Json;
    //     using var text = context.OpenResponseText(new UTF8Encoding(false));
    //     
    //     await text.WriteAsync(JsonSerializer.Serialize(data,
    //         new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })).ConfigureAwait(false);
    // }

    public Web(Env env, DatabaseManager db, DiscordManager discordManager)
    {
        _db = db;
        _env = env;
        _discordManager = discordManager;

        var container = new DependencyContainer();
        container.Register(discordManager);
        container.Register(db);
        container.Register(env);

        _server = new WebServer(o => o.WithUrlPrefix($"http://{env.ListenUrl}").WithMode(HttpListenerMode.EmbedIO))
            .WithLocalSessionManager();

        _server
            .WithCors()
            .WithBearerToken("/api/calendar", env.Secret)
            .WithBearerToken("/api/user", env.Secret)
            .WithWebApi("/api/auth", m => m.WithController(() => new AuthController().WithContainer(container)))
            .WithWebApi("/api/calendar", m => m.WithController(() => new CalendarController().WithContainer(container)))
            .WithWebApi("/api/user", m => m.WithController(() => new UserController().WithContainer(container)));
        if (env.StaticHtmlPath != null)
            _server.WithStaticFolder("/", env.StaticHtmlPath, false);
        _server.WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = ":)" })));

        // Listen for state changes.
        _server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();

        _server.RunAsync();
    }

    public void Dispose()
    {
        _server.Dispose();
        GC.SuppressFinalize(this);
    }
}