using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.BearerToken;
using EmbedIO.WebApi;
using Swan.Logging;
using XorusCalendarBot.Database;

namespace XorusCalendarBot.Api;

public class Web : IDisposable
{
    private readonly WebServer _server;
    private readonly DatabaseManager _db;
    private Env _env;

    public Web(Env env, DatabaseManager db)
    {
        _db = db;
        _env = env;

        _server = new WebServer(o => o.WithUrlPrefix($"http://{env.ListenUrl}").WithMode(HttpListenerMode.EmbedIO))
            .WithLocalSessionManager();

        _server
            .WithBearerToken("/api/calendar", env.Secret)
            .WithBearerToken("/api/user", env.Secret)
            .WithWebApi("/api/auth", m => m.WithController(() => new AuthController(_env, _db)))
            .WithWebApi("/api/calendar", m => m.WithController(() => new CalendarController(_db)))
            .WithWebApi("/api/user", m => m.WithController(() => new UserController(_db)))
            .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = ":)" })));

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