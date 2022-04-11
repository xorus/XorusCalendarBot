using EmbedIO;
using EmbedIO.WebApi;
using Swan.DependencyInjection;
using XorusCalendarBot.Database;
using XorusCalendarBot.Module.Calendar;

namespace XorusCalendarBot.Api;

public class BaseController : WebApiController
{
    public DependencyContainer Container { protected get; set; } = null!;

    protected UserEntity GetUserFromHttpContext()
    {
        var user = Container.Resolve<DatabaseManager>().GetUser(HttpContext.User.Identity?.Name ?? "");
        if (user == null) throw new HttpException(401);
        return user;
    }

    protected void EnsureSuperAdmin()
    {
        var user = GetUserFromHttpContext();
        if (user == null) throw new HttpException(401);
        if (!Container.Resolve<Env>().DiscordAdminId.Equals(user.DiscordId)) throw new HttpException(401);
    }

    protected void EnsureGuildAdmin(CalendarEntity calendarEntity)
    {
        var user = GetUserFromHttpContext();
        if (user == null) throw new HttpException(401);
        if (!Container.Resolve<Env>().DiscordAdminId.Equals(user.DiscordId)) throw new HttpException(401);
    }

    protected void EnsureEditor(CalendarEntity calendarEntity)
    {
        var user = GetUserFromHttpContext();
        if (user == null) throw new HttpException(401);
        if (!Container.Resolve<Env>().DiscordAdminId.Equals(user.DiscordId)) throw new HttpException(401);
    }
}