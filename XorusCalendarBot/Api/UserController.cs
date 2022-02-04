using ColorThiefDotNet;
using DSharpPlus;
using EmbedIO;
using EmbedIO.Routing;
using XorusCalendarBot.Database;
using XorusCalendarBot.Discord;

namespace XorusCalendarBot.Api;

public class UserController : BaseController
{
    [Route(HttpVerbs.Get, "/guilds")]
    public IEnumerable<GuildInfo> GetGuilds()
    {
        var dm = Container.Resolve<DiscordManager>();
        return dm.GetGuilds()
            .Where(x => GetUserFromHttpContext().Guilds.Contains(x.Value.Id.ToString()))
            .Select(x => new GuildInfo
            {
                Id = x.Key.ToString(),
                IconUrl = x.Value.IconUrl,
                Name = x.Value.Name,
                Channels = x.Value.Channels
                    .Where(c => !c.Value.IsThread
                                && c.Value.Type != ChannelType.Voice
                                && (c.Value.PermissionsFor(x.Value.CurrentMember) & Permissions.AccessChannels) != 0
                                && (c.Value.PermissionsFor(x.Value.CurrentMember) & Permissions.SendMessages) != 0)
                    .Select(y => new ChannelInfo()
                    {
                        Id = y.Value.Id.ToString(),
                        Name = y.Value.IsCategory ? y.Value.Name : "#" + y.Value.Name.ToLower().RemoveAccents(),
                        Category = y.Value.IsCategory
                    })
            });
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

    public struct ChannelInfo
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public bool Category { get; init; }
    }

    public struct GuildInfo
    {
        public string Id { get; init; }
        public string IconUrl { get; init; }
        public string Name { get; init; }
        public IEnumerable<ChannelInfo> Channels { get; init; }
    }
}