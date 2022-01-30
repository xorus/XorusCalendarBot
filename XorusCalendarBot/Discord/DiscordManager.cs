using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace XorusCalendarBot.Discord;

public class DiscordManager
{
    public readonly DiscordClient DiscordClient;
    public readonly Dictionary<string, IEnumerable<Instance>> InstanceDictionary = new();

    public DiscordManager(Env env)
    {
        DiscordClient = new DiscordClient(
            new DiscordConfiguration
            {
                Token = env.DiscordBotToken,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged
            }
        );
    }

    public async void Connect()
    {
        var slash = DiscordClient.UseSlashCommands(new SlashCommandsConfiguration
        {
            Services = new ServiceCollection()
                .AddSingleton(this)
                .BuildServiceProvider()
        });
        slash.SlashCommandErrored += (sender, args) =>
        {
            Console.WriteLine("Error: " + sender + ", " + args.Exception);
            return Task.CompletedTask;
        };

        // foreach (var keyValuePair in InstanceDictionary)
        // {
        // slash.RegisterCommands<SlashCommands>(keyValuePair.Key);
        // }

        slash.RegisterCommands<SlashCommands>();
        await DiscordClient.ConnectAsync();
    }

    public async Task<IEnumerable<Mention>?> GetAvailableMentions(string guildId)
    {
        var guild = await DiscordClient.GetGuildAsync(Convert.ToUInt64(guildId));
        return guild == null
            ? null
            : guild.Roles
                .Where(r => r.Value.IsMentionable)
                .Select(r => Mention.FromRole(r.Value));
    }

    public IReadOnlyDictionary<ulong, DiscordGuild> GetGuilds()
    {
        return DiscordClient.Guilds;
    }

    public void DisconnectSync()
    {
        DiscordClient.DisconnectAsync().Wait();
    }

    public class Mention
    {
        public string Name { get; private init; } = null!;
        public string Code { get; private init; } = null!;

        public static Mention FromRole(DiscordRole role)
        {
            return new Mention { Name = "@" + role.Name, Code = "<@&" + role.Id + ">" };
        }
    }
}