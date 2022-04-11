using EmbedIO;
using EmbedIO.WebApi;
using LiteDB;
using Swan.DependencyInjection;
using XorusCalendarBot.Api;
using XorusCalendarBot.Database;

namespace XorusCalendarBot.Module.Calendar;

public class CalendarModule : Base.Module
{
    private readonly InstanceDictionary _instanceDictionary;
    public readonly ILiteCollection<CalendarEntity> CalendarEntityCollection = null!;
    private readonly LiteDatabase _db;
    public event EventHandler? CalendarEntitiesUpdated;

    private int DbVersion
    {
        get => (int)_db.Pragma("MODULE_CALENDAR_VERSION");
        set => _db.Pragma("MODULE_CALENDAR_VERSION", value);
    }

    public CalendarModule(DependencyContainer container) : base(container)
    {
        _instanceDictionary = new InstanceDictionary(container);
        container.Register(_instanceDictionary);
        container.Register(this);
        _instanceDictionary.Init();

        _db = container.Resolve<DatabaseManager>().Database;
        Migrate();
        CalendarEntityCollection = _db.GetCollection<CalendarEntity>("calendars");
    }

    private void Migrate()
    {
        if (DbVersion == 0)
        {
            DbVersion = 1;
        }
    }

    public override void RegisterControllers(WebServer server)
    {
        server.WithWebApi("/api/calendar", Web.Serializer,
            m => m.WithController(() => new CalendarController().WithContainer(Container)));
    }

    public override void Dispose()
    {
        _instanceDictionary.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Update(CalendarEntity calendarEntity)
    {
        CalendarEntityCollection.Update(calendarEntity);
    }

    public void FireCalendarEntitiesUpdated()
    {
        CalendarEntitiesUpdated?.Invoke(this, EventArgs.Empty);
    }

    public List<CalendarEntity> GetUserCalendars(UserEntity user)
    {
        return CalendarEntityCollection
            .FindAll()
            .Where(c => user.Guilds.Contains(c.GuildId))
            .OrderBy(c => c.CreatedAt)
            .ToList();
    }
}