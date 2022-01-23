using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using XorusCalendarBot.Database;

namespace XorusCalendarBot.Api;

public class UserController : WebApiController
{
    private DatabaseManager _db;

    public UserController(DatabaseManager db)
    {
        _db = db;
    }

    [Route(HttpVerbs.Get, "/me")]
    public List<CalendarEntity> Me()
    {
        return _db.CalendarEntities.ToList();
    }
}