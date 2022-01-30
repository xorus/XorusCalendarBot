using EmbedIO;
using EmbedIO.Routing;
using Swan;
using Swan.Logging;
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

    public struct GuildInfo
    {
        public string Id { get; init; }
        public string IconUrl { get; init; }
        public string Name { get; init; }
    }
}