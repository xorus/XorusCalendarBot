using Discord;
using Discord.WebSocket;
using Swan.DependencyInjection;
using Swan.Logging;
using XorusCalendarBot.Database;

namespace XorusCalendarBot.Discord;

public class DiscordManager
{
    private readonly DependencyContainer _container;
    public readonly DiscordSocketClient DiscordClient;

    public DiscordManager(DependencyContainer container)
    {
        _container = container;

        DiscordClient = new DiscordSocketClient();
        DiscordClient.Log += Log;

        // DiscordClient = new DiscordCl ient(
        //     new DiscordConfiguration
        //     {
        //         Token = container.Resolve<Env>().DiscordBotToken,
        //         TokenType = TokenType.Bot,
        //         Intents = DiscordIntents.AllUnprivileged
        //     }
        // );
    }

    private static Task Log(LogMessage msg)
    {
        var message = msg.Severity + " " + msg.Message;
        if (msg.Exception != null)
        {
            message += " " + msg.Exception.Message;
            Logger.Error(message + "\n" + msg.Exception.ToString(), $"Discord/{msg.Source}");
        }

        var source = $"Discord/{msg.Source}";
        switch (msg.Severity)
        {
            case LogSeverity.Critical:
                message.Fatal(source);
                break;
            case LogSeverity.Error:
                message.Error(source);
                break;
            case LogSeverity.Warning:
                message.Warn(source);
                break;
            case LogSeverity.Info:
                message.Info(source);
                break;
            case LogSeverity.Verbose:
                message.Debug(source);
                break;
            case LogSeverity.Debug:
                message.Trace(source);
                break;
            default:
                message.Info(source);
                break;
        }

        return Task.CompletedTask;
    }

    public async void Connect()
    {
        await DiscordClient.LoginAsync(TokenType.Bot, _container.Resolve<Env>().DiscordBotToken);
        await DiscordClient.StartAsync();

        DiscordClient.GuildAvailable += guild =>
        {
            $"New guild available: {guild.Name} ({guild.Id})".Info("DiscordManager");
            return Task.CompletedTask;
        };

        // DiscordClient.Ready += () => Task.CompletedTask;

        // var slash = DiscordClient.UseSlashCommands(new SlashCommandsConfiguration
        // {
        //     Services = new ServiceCollection()
        //         .AddSingleton(this)
        //         .BuildServiceProvider()
        // });
        // slash.SlashCommandErrored += (sender, args) =>
        // {
        //     Console.WriteLine("Error: " + sender + ", " + args.Exception);
        //     return Task.CompletedTask;
        // };
        //
        //
        //
        // foreach (var keyValuePair in InstanceDictionary)
        // {
        //     slash.RegisterCommands<SlashCommands>(keyValuePair.Key);
        // }
        // slash.RegisterCommands<SlashCommands>();
    }

    public IEnumerable<Mention> GetAvailableMentions(string guildId)
    {
        var guild = DiscordClient.GetGuild(Convert.ToUInt64(guildId));
        var canMentionAnyone = guild.CurrentUser.GuildPermissions.Has(GuildPermission.MentionEveryone);

        var mentions = new List<Mention>();
        // foreach (var user in guild.Users)
        // {
        //     mentions.AddRange(Mention.FromUser(user));
        // }
        mentions.AddRange(
            from role in guild.Roles
            where canMentionAnyone || role.IsMentionable
            select Mention.FromRole(role)
        );
        return mentions;
    }

    public IReadOnlyDictionary<ulong, IGuild> GetGuilds()
    {
        return DiscordClient.Guilds.ToDictionary<SocketGuild?, ulong, IGuild>(g => g.Id, g => g);
    }

    public SocketGuild? GetGuild(IChannel channel)
    {
        return (from discordClientGuild in DiscordClient.Guilds
            from socketGuildChannel in discordClientGuild.Channels
            where socketGuildChannel.Equals(channel)
            select discordClientGuild).FirstOrDefault();
    }

    // public void PrefetchGuilds()
    // {
    //     foreach (var user in _container.Resolve<DatabaseManager>().Users.FindAll())
    //     {
    //         foreach (var userGuild in user.Guilds)
    //         {
    //             var guild = DiscordClient.Guild(ulong.Parse(userGuild));
    //             if (guild == null)
    //             {
    //                 $"Could not fetch guild {userGuild}".Error(nameof(DiscordManager));
    //             }
    //             else
    //             {
    //                 $"Fetched guild {guild.Id}".Info(nameof(DiscordManager));
    //             }
    //         }
    //     }
    // }

    public void DisconnectSync()
    {
        DiscordClient.StopAsync().Wait();
    }

    public class Mention
    {
        public string Name { get; private init; } = null!;
        public string Code { get; private init; } = null!;

        public static Mention FromRole(IRole role)
        {
            return role.Name == "@everyone"
                ? new Mention { Name = role.Name, Code = role.Name }
                : new Mention { Name = "@" + role.Name, Code = "<@&" + role.Id + ">" };
        }

        public static Mention[] FromUser(SocketGuildUser user)
        {
            return new[]
            {
                new Mention { Name = "@" + user.Username, Code = user.Mention },
                new Mention { Name = "@" + user.Username + "#" + user.DiscriminatorValue, Code = user.Mention }
            };
        }
    }
}