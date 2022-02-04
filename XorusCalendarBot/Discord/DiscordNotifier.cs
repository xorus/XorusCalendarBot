using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using FluentScheduler;
using Swan.Logging;
using XorusCalendarBot.Cal;
using XorusCalendarBot.Database;

namespace XorusCalendarBot.Discord;

public class DiscordNotifier : IDisposable
{
    private readonly DiscordManager _discord;
    private readonly Instance _instance;

    public DiscordNotifier(Instance instance)
    {
        _instance = instance;
        _discord = instance.Container.Resolve<DiscordManager>();
    }

    public void Dispose()
    {
        UnregisterJobs();
        GC.SuppressFinalize(this);
    }

    public void RegisterJobs()
    {
        UnregisterJobs();

#if DEBUG
        var i = 0;
#endif
        foreach (var occurrence in _instance.CalendarEntity.NextOccurrences)
        {
            // var runAt = occurrence.StartTime
            // .Add(TimeSpan.FromSeconds(_instance.CalendarEntity.ReminderOffsetSeconds));
            var runAt = occurrence.NotifyTime;

#if DEBUG
            if (i == 0)
            {
                runAt = DateTime.Now + TimeSpan.FromSeconds(5 + 5 * i);
                i++;
            }
            //
            // Console.WriteLine(runAt);
#endif
            // Skip already passed event
            if (DateTime.Now > runAt) continue;

            JobManager.AddJob(() => { Notify(occurrence); },
                schedule => schedule
                    .WithName("discord-notify-" + _instance.CalendarEntity.Id + "-" + occurrence.StartTime)
                    .ToRunOnceAt(runAt));
        }
    }

    public void UnregisterJobs()
    {
        foreach (var b in JobManager.AllSchedules.Where(s =>
                     s.Name.StartsWith("discord-notify-" + _instance.CalendarEntity.Id + "-")))
            JobManager.RemoveJob(b.Name);
    }

    private async void Notify(EventOccurrence occurrence)
    {
        if (string.IsNullOrEmpty(_instance.CalendarEntity.ReminderChannel)) return;
        DiscordChannel channel;
        try
        {
            channel = await GetChannel(Convert.ToUInt64(_instance.CalendarEntity.ReminderChannel));
        }
        catch (NotFoundException)
        {
            ("Channel " + _instance.CalendarEntity.ReminderChannel + "does not exist").Error();
            return;
        }

        string? str = null;
        if (occurrence.IsForced) str = occurrence.Message;
        str ??= GetNextMessage(_instance.CalendarEntity);

        var mentions = await _discord.GetAvailableMentions(_instance.CalendarEntity.GuildId);

        if (mentions != null)
        {
            str = mentions.Aggregate(str, (current, mm) => current.Replace(mm.Name, mm.Code));
        }

        str = new[] { 't', 'T', 'd', 'D', 'f', 'F', 'R' }.Aggregate(str,
            (current, format) =>
                current.Replace($"<{format}>", $"<t:{occurrence.StartTime.ToUnixTimestamp()}:{format}>")
        );

        await new DiscordMessageBuilder()
            .WithAllowedMention(RoleMention.All)
            .WithAllowedMention(UserMention.All)
            .WithAllowedMention(EveryoneMention.All)
            .WithContent(str)
            .SendAsync(channel);

        _instance.Update();
    }

    public static string GetNextMessage(CalendarEntity calendarEntity, int? withIndex = null)
    {
        if (calendarEntity.Sentences.Count == 0) return "";
        var i = (calendarEntity.NextSentence + (withIndex ?? 0)) % calendarEntity.Sentences.Count;
        var str = calendarEntity.Sentences[i];
        if (withIndex != null) return str;
        calendarEntity.NextSentence = (i + 1) % calendarEntity.Sentences.Count;
        return str;
    }

    private async Task<DiscordChannel> GetChannel(ulong channel)
    {
        // todo: handle channel not existing
        return await _instance.Container.Resolve<DiscordManager>().DiscordClient.GetChannelAsync(channel);
    }
}