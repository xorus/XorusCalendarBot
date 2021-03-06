using System.Text;
using EmbedIO;
using EmbedIO.BearerToken;
using EmbedIO.WebApi;
using Newtonsoft.Json;
using Swan.DependencyInjection;
using Swan.Logging;
using XorusCalendarBot.Module.Base;

namespace XorusCalendarBot.Api;

public class Web : IDisposable
{
    private readonly WebServer _server;

    public Web(DependencyContainer container, List<IModule> modules)
    {
        var env = container.Resolve<Env>();
        _server = new WebServer(o => o.WithUrlPrefix($"http://{env.ListenUrl}").WithMode(HttpListenerMode.EmbedIO))
            .WithLocalSessionManager();

        _server
            .WithCors()
            .WithBearerToken("/api/user", env.Secret)
            .WithWebApi("/api/auth", Serializer,
                m => m.WithController(() => new AuthController().WithContainer(container)))
            .WithWebApi("/api/user", Serializer,
                m => m.WithController(() => new UserController().WithContainer(container)));
        foreach (var module in modules) module.RegisterControllers(_server);
        if (env.StaticHtmlPath != null)
            _server.WithStaticFolder("/", env.StaticHtmlPath, false);

        // Listen for state changes.
        _server.StateChanged += (s, e) => $"Now {e.NewState}".Info("WebServer");
        _server.RunAsync();
    }

    public void Dispose()
    {
        _server.Dispose();
        GC.SuppressFinalize(this);
    }

    public static Task Serializer(IHttpContext context, object? data)
    {
        return context.SendStringAsync(JsonConvert.SerializeObject(data), "application/json", Encoding.UTF8);
    }
}