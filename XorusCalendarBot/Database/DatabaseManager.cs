using LiteDB;
using Swan.DependencyInjection;

namespace XorusCalendarBot.Database;

public class DatabaseManager : IDisposable
{
    public readonly LiteDatabase Database;

    public readonly ILiteCollection<UserEntity> Users;

    public DatabaseManager(DependencyContainer container)
    {
        var env = container.Resolve<Env>();
        Database = new LiteDatabase(env.DatabasePath ?? "database.db");

        Users = Database.GetCollection<UserEntity>("users");
        if (Users.Count() == 0)
            Users.Insert(new UserEntity { DiscordId = env.DiscordAdminId, IsAdmin = true });

        new Migration(container).DoIt(Database);
    }

    public void Dispose()
    {
        Database.Dispose();
        GC.SuppressFinalize(this);
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