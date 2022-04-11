using EmbedIO;
using Swan.DependencyInjection;

namespace XorusCalendarBot.Module.Base;

public abstract class Module : IDisposable
{
    protected DependencyContainer Container;

    protected Module(DependencyContainer container)
    {
        Container = container;
    }

    public abstract void Dispose();

    public virtual void RegisterControllers(WebServer server)
    {
    }
}