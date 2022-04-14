using EmbedIO;
using Swan.DependencyInjection;
using Swan.Logging;
using XorusCalendarBot.Database;

namespace XorusCalendarBot.Module.Base;

public abstract class Module : IModule
{
    protected DependencyContainer Container { get; }

    protected Module(DependencyContainer container)
    {
        Container = container;
    }

    public abstract void Dispose();

    public virtual void RegisterControllers(WebServer server)
    {
    }

    protected void Migrate()
    {
        var dbm = Container.Resolve<DatabaseManager>();
        var version = dbm.GetModuleVersion(GetName(), GetSchemaVersion());
        $"{GetName()} db={version}".Debug($"Module/{GetName()}");
        for (var i = version; i < GetSchemaVersion(); i++)
        {
            $"Migrating {GetName()}: {GetSchemaVersion()} => {i}".Info($"Module/{GetName()}");
            ApplyMigration(i);
            dbm.SetModuleVersion(GetName(), i);
        }
    }

    protected abstract string GetName();
    protected abstract int GetSchemaVersion();
    protected abstract void ApplyMigration(int toVersion);
}