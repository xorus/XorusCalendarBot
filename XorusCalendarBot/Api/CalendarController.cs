using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using XorusCalendarBot.Database;

namespace XorusCalendarBot.Api;

public class CalendarController : AbstractController
{
    public CalendarController(DatabaseManager database) : base(database)
    {
    }

    [Route(HttpVerbs.Get, "/")]
    public List<CalendarEntity> Index()
    {
        return Database.GetUserCalendars(GetUserFromHttpContext());
    }
}