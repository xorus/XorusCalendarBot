using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using XorusCalendarBot.Database;

namespace XorusCalendarBot.Api;

public class CalendarController : BaseController
{
    private DatabaseManager Database => Container.Resolve<DatabaseManager>();

    [Route(HttpVerbs.Get, "/")]
    public List<CalendarEntity> Index()
    {
        return Database.GetUserCalendars(GetUserFromHttpContext());
    }

    [Route(HttpVerbs.Get, "/{id}")]
    public CalendarEntity Get(string id)
    {
        try
        {
            return Database.GetUserCalendars(GetUserFromHttpContext()).First(c => c.Id.Equals(Guid.Parse(id)));
        }
        catch (Exception)
        {
            throw new HttpException(404);
        }
    }

    [Route(HttpVerbs.Put, "/{id}")]
    public CalendarEntity Update(string id, [JsonData] CalendarEntity calendar)
    {
        Console.WriteLine("yo" + calendar);
        if (calendar == null) throw new HttpException(400);
        if (!calendar.Id.Equals(Guid.Parse(id))) throw new HttpException(401);
        if (!GetUserFromHttpContext().Guilds.Contains(calendar.GuildId)) throw new HttpException(401);

        Database.Update(calendar);
        return calendar;
    }

    [Route(HttpVerbs.Delete, "/{id}")]
    public void Delete(string id)
    {
        try
        {
            var calendar = Database.CalendarEntityCollection.FindById(Guid.Parse(id));
            if (!GetUserFromHttpContext().Guilds.Contains(calendar.GuildId)) throw new HttpException(401);
            Database.CalendarEntityCollection.Delete(Guid.Parse(id));
        }
        catch (Exception)
        {
            throw new HttpException(404);
        }
    }

    [Route(HttpVerbs.Post, "/")]
    public CalendarEntity Create([QueryField] string guild)
    {
        var calendar = new CalendarEntity();
        calendar.Id = Guid.NewGuid();
        calendar.GuildId = guild;
        Database.CalendarEntityCollection.Insert(calendar);
        return calendar;
    }
}