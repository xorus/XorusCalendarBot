using System.Runtime.CompilerServices;
using EmbedIO;
using Swan.DependencyInjection;
using Swan.Logging;
using XorusCalendarBot.Database;

namespace XorusCalendarBot.Module.Base;

public abstract class Module : IModule
{
    protected Module(DependencyContainer container)
    {
        Container = container;
    }

    protected DependencyContainer Container { get; }

    public abstract void Dispose();

    public virtual void RegisterControllers(WebServer server)
    {
    }

    protected void Migrate()
    {
        var dbm = Container.Resolve<DatabaseManager>();
        var version = dbm.GetModuleVersion(GetName().ToLowerInvariant(), GetSchemaVersion());
        $"{GetName()} db={version}".Debug($"Module/{GetName()}");
        for (var i = version; i < GetSchemaVersion(); i++)
        {
            $"Migrating {GetName()}: {GetSchemaVersion()} => {i}".Info($"Module/{GetName()}");
            ApplyMigration(i);
            dbm.SetModuleVersion(GetName().ToLowerInvariant(), i);
        }
    }

    public abstract string GetName();
    protected abstract int GetSchemaVersion();
    protected abstract void ApplyMigration(int toVersion);
}