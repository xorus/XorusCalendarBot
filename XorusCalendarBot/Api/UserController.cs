using Discord;
using Discord.WebSocket;
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
            .Select(x =>
            {
                var channels = x.Value.GetChannelsAsync().Result;
                var botUser = x.Value.GetCurrentUserAsync(CacheMode.CacheOnly).Result;

                return new GuildInfo
                {
                    Id = x.Key.ToString(),
                    IconUrl = x.Value.IconUrl,
                    Name = x.Value.Name,
                    Channels = channels
                        .Where(c =>
                        {
                            if (c is not SocketCategoryChannel && c is not SocketTextChannel) return false;

                            var perms = botUser.GetPermissions(c);
                            return perms.ViewChannel && perms.SendMessages;
                        })
                        .Select(y =>
                        {
                            if (y is SocketCategoryChannel)
                                return new ChannelInfo
                                {
                                    Id = y.Id.ToString(),
                                    Name = y.Name,
                                    Category = true
                                };

                            if (y is SocketTextChannel)
                                return new ChannelInfo
                                {
                                    Id = y.Id.ToString(),
                                    Name = "#" + y.Name.ToLower().RemoveAccents(),
                                    Category = false
                                };

                            throw new Exception("Unexpected channel type " + y.GetType().Name);
                        })
                };
            });
    }

    [Route(HttpVerbs.Get, "/user")]
    public IEnumerable<UserEntity> ListUsers()
    {
        EnsureSuperAdmin();
        return Container.Resolve<DatabaseManager>().Users.FindAll();
    }


    [Route(HttpVerbs.Post, "/user/{id}")]
    public IEnumerable<UserEntity> AddUser(string id)
    {
        EnsureSuperAdmin();
        Container.Resolve<DatabaseManager>().Users.Insert(new UserEntity
        {
            DiscordId = id
        });
        return Container.Resolve<DatabaseManager>().Users.FindAll();
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