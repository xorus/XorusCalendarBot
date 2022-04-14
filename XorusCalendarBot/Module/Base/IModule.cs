using EmbedIO;

namespace XorusCalendarBot.Module.Base;

public interface IModule : IDisposable
{
    public void RegisterControllers(WebServer server);
}