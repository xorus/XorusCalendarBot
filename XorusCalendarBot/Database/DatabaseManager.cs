using LiteDB;
using Swan.DependencyInjection;

namespace XorusCalendarBot.Database;

public class DatabaseManager : IDisposable
{
    private readonly ILiteCollection<ModuleEntity> _modules;
    public readonly LiteDatabase Database;
    public readonly ILiteCollection<UserEntity> Users;

    public DatabaseManager(DependencyContainer container)
    {
        var env = container.Resolve<Env>();
        Database = new LiteDatabase(env.DatabasePath ?? "database.db");
        Database.UserVersion = 0;

        Users = Database.GetCollection<UserEntity>("users");
        if (Users.Count() == 0)
            Users.Insert(new UserEntity { DiscordId = env.DiscordAdminId, IsAdmin = true });

        _modules = Database.GetCollection<ModuleEntity>("modules");
    }

    public void Dispose()
    {
        Database.Dispose();
        GC.SuppressFinalize(this);
    }

    public int GetModuleVersion(string name, int currentVersion)
    {
        var module = _modules.FindOne(x => x.Id == name);
        if (module != null) return module.Version;

        module = new ModuleEntity { Id = name, Version = currentVersion };
        _modules.Insert(module);
        return module.Version;
    }

    public void SetModuleVersion(string name, int version)
    {
        var module = _modules.FindOne(x => x.Id == name);
        if (module == null) throw new ArgumentException("Module " + name + " not found");
        module.Version = version;
        _modules.Update(module);
    }

    public UserEntity? GetUser(string guid)
    {
        try
        {
            return Users.FindById(Guid.Parse(guid));
        }
        catch (Exception)
        {
            return null;
        }
    }
}