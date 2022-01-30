using System.Text;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.BearerToken;
using EmbedIO.WebApi;
using Newtonsoft.Json;
using Swan.DependencyInjection;
using Swan.Logging;
using XorusCalendarBot.Database;
using XorusCalendarBot.Discord;

namespace XorusCalendarBot.Api;

public class Web : IDisposable
{
    private readonly WebServer _server;

    // public static async Task SerializationCallback(IHttpContext context, object? data)
    // {
    //     Validate.NotNull(nameof(context), context).Response.ContentType = MimeType.Json;
    //     using var text = context.OpenResponseText(new UTF8Encoding(false));
    //     
    //     await text.WriteAsync(JsonSerializer.Serialize(data,
    //         new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })).ConfigureAwait(false);
    // }

    private static Task Serializer(IHttpContext context, object? data)
    {
        return context.SendStringAsync(JsonConvert.SerializeObject(data), "application/json", Encoding.UTF8);
    }

    public Web(DependencyContainer container)
    {
        var env = container.Resolve<Env>();
        _server = new WebServer(o => o.WithUrlPrefix($"http://{env.ListenUrl}").WithMode(HttpListenerMode.EmbedIO))
            .WithLocalSessionManager();

        _server
            .WithCors()
            .WithBearerToken("/api/calendar", env.Secret)
            .WithBearerToken("/api/user", env.Secret)
            .WithWebApi("/api/auth", Serializer,
                m => m.WithController(() => new AuthController().WithContainer(container)))
            .WithWebApi("/api/calendar", Serializer,
                m => m.WithController(() => new CalendarController().WithContainer(container)))
            .WithWebApi("/api/user", Serializer,
                m => m.WithController(() => new UserController().WithContainer(container)));
        if (env.StaticHtmlPath != null)
            _server.WithStaticFolder("/", env.StaticHtmlPath, false);

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