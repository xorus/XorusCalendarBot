using EmbedIO;
using EmbedIO.BearerToken;
using EmbedIO.WebApi;
using LiteDB;
using Swan.DependencyInjection;
using Swan.Logging;
using XorusCalendarBot.Api;
using XorusCalendarBot.Database;

namespace XorusCalendarBot.Module.Calendar;

public class CalendarModule : Base.Module
{
    private readonly InstanceDictionary _instanceDictionary;
    public readonly ILiteCollection<CalendarEntity> CalendarEntityCollection;

    public CalendarModule(DependencyContainer container) : base(container)
    {
        Migrate();
        CalendarEntityCollection = Container.Resolve<DatabaseManager>().Database
            .GetCollection<CalendarEntity>("calendars");

        Container.Register(this);
        _instanceDictionary = new InstanceDictionary(Container);
        Container.Register(_instanceDictionary);
        _instanceDictionary.Init();
    }

    public event EventHandler? CalendarEntitiesUpdated;

    protected override void ApplyMigration(int toVersion)
    {
        Console.WriteLine("Apply migration to " + toVersion);
        // var adminId = _container.Resolve<Env>().DiscordAdminId;
        //
        // var userCollection = db.GetCollection<BsonDocument>("users");
        // var docs = userCollection.FindAll();
        // foreach (var bsonDocument in docs)
        // {
        //     if (bsonDocument.ContainsKey("DiscordId") && adminId == bsonDocument["DiscordId"].AsString)
        //     {
        //         bsonDocument["IsAdmin"] = new BsonValue(true);
        //         userCollection.Update(bsonDocument);
        //     }
        // }
    }

    public override void RegisterControllers(WebServer server)
    {
        "Registering calendar controller".Info(GetName());
        server.WithBearerToken("/api/calendar", Container.Resolve<Env>().Secret);
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

    public sealed override string GetName()
    {
        return "Calendar";
    }

    protected override int GetSchemaVersion()
    {
        return 0;
    }
}