using Discord;
using Discord.WebSocket;
using FuzzySharp;
using Swan.DependencyInjection;
using Swan.Logging;
using XorusCalendarBot.Discord;
using XorusCalendarBot.Module.Soundboard.Entity;

namespace XorusCalendarBot.Module.Soundboard;

public class DiscordCommand
{
    private readonly SoundboardModule _soundboardModule;
    private readonly DependencyContainer _container;
    private readonly DiscordManager _discord;
    private readonly Dictionary<SocketGuild, List<SoundEntity>> _sounds = new();
    private readonly SoundPlayer _player;

    public DiscordCommand(DependencyContainer container)
    {
        _container = container;
        _soundboardModule = container.Resolve<SoundboardModule>();
        _discord = container.Resolve<DiscordManager>();
        _player = new SoundPlayer(container);

        _discord.DiscordClient.SlashCommandExecuted += DiscordClientOnSlashCommandExecuted;
        _discord.DiscordClient.GuildAvailable += DiscordClientOnGuildAvailable;
        _discord.DiscordClient.AutocompleteExecuted += DiscordClientOnAutocompleteExecuted;
    }

    public void UpdateSoundsList(SocketGuild guild)
    {
        if (_sounds.ContainsKey(guild)) _sounds[guild].Clear();
        else _sounds[guild] = new List<SoundEntity>();
        foreach (var x1 in _soundboardModule.SoundCollection.FindAll())
        {
            if (x1.GuildId == null || !ulong.Parse(x1.GuildId).Equals(guild.Id)) continue;
            _sounds[guild].Add(x1);
        }
    }

    private IEnumerable<SoundEntity> SearchSound(SocketGuild guild, string search)
    {
        if (search.Trim().Length == 0)
        {
            return new List<SoundEntity>();
        }

        var results = new Dictionary<SoundEntity, float>();
        // "---".Info();
        foreach (var soundEntity in _sounds[guild])
        {
            var slug = soundEntity.GetSlug();
            if (slug == null) continue;
            var distance = Fuzz.Ratio(slug, search) / 2 + Fuzz.PartialTokenSetRatio(slug, search) / 2;
            results.Add(soundEntity, distance);
            // var a = "|" + search + "|";
            // a += " a" + Fuzz.Ratio(slug, arg.Data.Current.Value.ToString());
            // a += " b" + Fuzz.PartialRatio(slug, arg.Data.Current.Value.ToString());
            // a += " c" + Fuzz.TokenSortRatio(slug, arg.Data.Current.Value.ToString());
            // a += " d" + Fuzz.PartialTokenSortRatio(slug, arg.Data.Current.Value.ToString());
            // a += " e" + Fuzz.TokenSetRatio(slug, arg.Data.Current.Value.ToString());
            // a += " f" + Fuzz.PartialTokenSetRatio(slug, arg.Data.Current.Value.ToString());
            // a += " g" + Fuzz.TokenInitialismRatio(slug, arg.Data.Current.Value.ToString());
            // a += " h" + Fuzz.TokenAbbreviationRatio(slug, arg.Data.Current.Value.ToString());
            //
            // $"{slug} - {distance} - {a}".Info();
        }

        return results.OrderByDescending(x => x.Value)
            .Where(x => x.Value > 50)
            .Select(kvp => kvp.Key).ToList();
    }

    private async Task DiscordClientOnAutocompleteExecuted(SocketAutocompleteInteraction arg)
    {
        // arg.Data.ToJson().Info();
        // return;
        if (arg.Data.CommandName != "sfx") return;

        var guild = _discord.GetGuild(arg.Channel);
        if (guild == null || !_sounds.ContainsKey(guild))
        {
            await arg.RespondAsync("An error occured while trying to find the guild.");
            return;
        }

        await arg.RespondAsync(SearchSound(guild, arg.Data.Current.Value.ToString() ?? "").Select(x =>
            new AutocompleteResult
            {
                Name = x.GetSlug(),
                Value = x.GetSlug()
            }));
    }

    // private async Task Register()
    // {
    //     "Registering commands".Info($"{_soundboardModule.GetName()}/DiscordCommand");
    //     foreach (var kvp in _discord.GetGuilds())
    //     {
    //         await this.Register((SocketGuild)kvp.Value);
    //     }
    // }

    private async Task Register(SocketGuild guild)
    {
        // await guild.GetApplicationCommandsAsync()
        return;
        try
        {
            $"Registering /sfx command for guild {guild.Name}".Info($"{_soundboardModule.GetName()}/DiscordCommand");
            var command = new SlashCommandBuilder();
            command
                .WithName("sfx")
                .WithDescription("Play a sound effect")
                .AddOption("play", ApplicationCommandOptionType.String, "Play a sound effect", false, true, true);
            await guild.CreateApplicationCommandAsync(command.Build());
        }
        catch (Exception e)
        {
            e.Message.Fatal($"{_soundboardModule.GetName()}/DiscordCommand");
        }
    }

    private Task DiscordClientOnGuildAvailable(SocketGuild guild)
    {
        UpdateSoundsList(guild);
        return Register(guild);
    }

    private Task DiscordClientOnSlashCommandExecuted(SocketSlashCommand command)
    {
        if (command.Data.Name != "sfx") return Task.CompletedTask;

        var guild = _discord.GetGuild(command.Channel);
        if (guild == null || !_sounds.ContainsKey(guild))
        {
            return command.RespondAsync("An internal error occured :(", ephemeral: true);
        }

        var soundOption = command.Data.Options.FirstOrDefault(x => x.Name == "play");
        if (soundOption == null)
        {
            return command.RespondAsync("Invalid arguments", ephemeral: true);
        }

// Where(x => x.GetSlug() != null).FirstOrDefault(x => x.GetSlug() == soundOption.Value);
        var sound = _sounds[guild].Find(x =>
        {
            var slug = x.GetSlug();
            if (slug == null) return false;
            return slug == soundOption.Value.ToString();
        });
        if (sound == null)
        {
            sound = SearchSound(guild, soundOption.Value.ToString() ?? "").FirstOrDefault();
            if (sound == null)
            {
                return command.RespondAsync($"No sound matches {soundOption.Value}.", ephemeral: true);
            }
        }

        if (!sound.Enabled) return command.RespondAsync($"This sound is disabled", ephemeral: true);
        // if (!sound.Last) return command.RespondAsync($"This sound is disabled", ephemeral: true);

        command.RespondAsync($"Playing {sound.GetSlug()}", ephemeral: true);
        _player.Play(guild, command.User, sound);

        return Task.CompletedTask;
    }
}