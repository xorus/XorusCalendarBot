using System.Diagnostics;
using Discord;
using Discord.Audio;
using Discord.WebSocket;
using Swan.DependencyInjection;
using Swan.Logging;
using XorusCalendarBot.Discord;
using XorusCalendarBot.Module.Soundboard.Entity;

namespace XorusCalendarBot.Module.Soundboard;

public class SoundPlayer
{
    private readonly DiscordSocketClient _discord;
    private readonly Dictionary<IGuild, IVoiceChannel> _connectedVoices = new();
    private readonly string Name;

    public SoundPlayer(DependencyContainer container)
    {
        _discord = container.Resolve<DiscordManager>().DiscordClient;
        Name = $"{container.Resolve<SoundboardModule>().GetName()}/SoundPlayer";
    }

    public async void Play(SocketGuild guild, SocketUser user, SoundEntity sound)
    {
        foreach (var socketGuildChannel in guild.Channels)
        {
            if (socketGuildChannel is not IVoiceChannel channel || !socketGuildChannel.Users.Contains(user)) continue;

            $"you are in {socketGuildChannel.Name}".Info();

            var connect = !(_connectedVoices.ContainsKey(guild) && channel.Equals(_connectedVoices[guild]));
            if (_connectedVoices.ContainsKey(guild) && !channel.Equals(_connectedVoices[guild]))
            {
                await _connectedVoices[guild].DisconnectAsync();
                _connectedVoices.Remove(guild);
                connect = true;
            }


            if (connect)
            {
                var client = await channel.ConnectAsync();
                client.Disconnected += _ => OnDisconnected(guild, client);
                client.Connected += () =>
                {
                    _connectedVoices.Add(guild, channel);
                    return Task.CompletedTask;
                };
                // client.Connected += _ => OnConnected(guild, client, sound);
                OnConnected(guild, client, sound);
            }

            // TODO: ffmpeg

            break;
        }
    }

    private Process? CreateStream(string path)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments =
                $"-hide_banner -loglevel panic -i \"{path}\" -filter:a \"volume=0.5\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
        });
    }

    private async void OnConnected(SocketGuild guild, IAudioClient client, SoundEntity sound)
    {
        $"Voice connected to {guild.Name}".Info(Name);
        if (sound.Uri == null)
        {
            $"No uri".Error(Name);
            return;
        }

        using var ffmpeg = CreateStream(sound.Uri);

        if (ffmpeg == null)
        {
            $"Could not create FFMPEG stream for {sound.Uri}".Error(Name);
            return;
        }

        await using var output = ffmpeg.StandardOutput.BaseStream;
        await using var discord = client.CreatePCMStream(AudioApplication.Mixed);
        try
        {
            await output.CopyToAsync(discord);
        }
        finally
        {
            await discord.FlushAsync();
            await client.StopAsync();
        }
    }

    private Task OnDisconnected(IGuild guild, IAudioClient audioClient)
    {
        _connectedVoices.Remove(guild);
        return Task.CompletedTask;
    }
}