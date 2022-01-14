using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using XorusCalendarBot.Configuration;

namespace XorusCalendarBot.Discord;

public class DiscordManager
{
    public readonly DiscordClient DiscordClient;
    public readonly Dictionary<ulong, Instance> InstanceDictionary = new();

    public DiscordManager(ConfigurationManager configurationManager)
    {
        DiscordClient = new DiscordClient(
            new DiscordConfiguration
            {
                Token = configurationManager.EnvConfig.DiscordBotToken,
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

    public void DisconnectSync()
    {
        DiscordClient.DisconnectAsync().Wait();
    }
}