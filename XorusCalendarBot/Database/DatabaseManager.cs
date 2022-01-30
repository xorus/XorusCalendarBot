using LiteDB;

namespace XorusCalendarBot.Database;

public class DatabaseManager : IDisposable
{
    private readonly LiteDatabase _db;

    public readonly ILiteCollection<CalendarEntity> CalendarEntityCollection;
    public readonly ILiteCollection<UserEntity> Users;

    public DatabaseManager(Env env)
    {
        _db = new LiteDatabase(env.DatabasePath ?? "database.db");
        CalendarEntityCollection = _db.GetCollection<CalendarEntity>("calendars");

        Users = _db.GetCollection<UserEntity>("users");
        if (Users.Count() == 0)
            Users.Insert(new UserEntity
            {
                DiscordId = env.DiscordAdminId
            });
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Update(CalendarEntity calendarEntity)
    {
        CalendarEntityCollection.Update(calendarEntity);
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

    public List<CalendarEntity> GetUserCalendars(UserEntity user)
    {
        return CalendarEntityCollection.FindAll().Where(c => user.Guilds.Contains(c.GuildId)).ToList();
    }
}