using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using XorusCalendarBot.Api;

namespace XorusCalendarBot.Module.Calendar;

public class CalendarController : BaseController
{
    private CalendarModule Database => Container.Resolve<CalendarModule>();

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

    [Route(HttpVerbs.Get, "/{id}/refresh")]
    public async Task<List<CalendarEntity>> Refresh(string id)
    {
        try
        {
            await Container.Resolve<InstanceDictionary>().RefreshAsync(Guid.Parse(id));
            return Index();
            // return Database.GetUserCalendars(GetUserFromHttpContext()).First(c => c.Id.Equals(Guid.Parse(id)));
        }
        catch (Exception)
        {
            throw new HttpException(404);
        }
    }

    [Route(HttpVerbs.Put, "/{id}")]
    public async Task<CalendarEntity> Update(string id, [JsonData] CalendarEntity calendar)
    {
        if (calendar == null) throw new HttpException(400);
        if (!calendar.Id.Equals(Guid.Parse(id))) throw new HttpException(401);
        if (!GetUserFromHttpContext().Guilds.Contains(calendar.GuildId)) throw new HttpException(401);

        Database.CalendarEntityCollection.Update(calendar);
        await Container.Resolve<InstanceDictionary>().RefreshAsync(calendar);
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
            Database.FireCalendarEntitiesUpdated();
        }
        catch (Exception)
        {
            throw new HttpException(404);
        }
    }

    [Route(HttpVerbs.Post, "/")]
    public CalendarEntity Create([QueryField] string guild)
    {
        var calendar = new CalendarEntity
        {
            Id = Guid.NewGuid(),
            GuildId = guild
        };
        Database.CalendarEntityCollection.Insert(calendar);
        Database.FireCalendarEntitiesUpdated();
        return calendar;
    }
}