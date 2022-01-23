using EmbedIO;
using EmbedIO.WebApi;
using XorusCalendarBot.Database;

namespace XorusCalendarBot.Api;

public abstract class AbstractController : WebApiController
{
    protected DatabaseManager Database;

    protected AbstractController(DatabaseManager database)
    {
        Database = database;
    }

    protected UserEntity GetUserFromHttpContext()
    {
        var user = Database.GetUser(HttpContext.User.Identity?.Name ?? "");
        if (user == null) throw new HttpException(401);
        return user;
    }
}