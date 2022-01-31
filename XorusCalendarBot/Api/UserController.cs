using EmbedIO;
using EmbedIO.Routing;
using Swan;
using Swan.Logging;
using XorusCalendarBot.Database;
using XorusCalendarBot.Discord;

namespace XorusCalendarBot.Api;

public class UserController : BaseController
{
    [Route(HttpVerbs.Get, "/guilds")]
    public IEnumerable<GuildInfo> GetGuilds()
    {
        var dm = Container.Resolve<DiscordManager>();
        ("amogus " + GetUserFromHttpContext().Guilds.ToJson()).Info();
        return dm.GetGuilds()
            .Where(x => GetUserFromHttpContext().Guilds.Contains(x.Value.Id.ToString()))
            .Select(x => new GuildInfo { Id = x.Key.ToString(), IconUrl = x.Value.IconUrl, Name = x.Value.Name });
    }

    [Route(HttpVerbs.Get, "/user")]
    public IEnumerable<UserEntity> ListUsers()
    {
        EnsureAdmin();
        return Container.Resolve<DatabaseManager>().Users.FindAll();
    }


    [Route(HttpVerbs.Post, "/user/{id}")]
    public IEnumerable<UserEntity> AddUser(string id)
    {
        EnsureAdmin();
        Container.Resolve<DatabaseManager>().Users.Insert(new UserEntity
        {
            DiscordId = id
        });
        return Container.Resolve<DatabaseManager>().Users.FindAll();
    }

    private void EnsureAdmin()
    {
        var user = GetUserFromHttpContext();
        if (user == null) throw new HttpException(401);
        if (!Container.Resolve<Env>().DiscordAdminId.Equals(user.DiscordId)) throw new HttpException(401);
    }

    public struct GuildInfo
    {
        public string Id { get; init; }
        public string IconUrl { get; init; }
        public string Name { get; init; }
    }
}