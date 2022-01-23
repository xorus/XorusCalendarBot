using DSharpPlus.Entities;
using FluentScheduler;
using Ical.Net.CalendarComponents;

namespace XorusCalendarBot.Discord;

public class DiscordNotifier : IDisposable
{
    private readonly Instance _instance;

    public DiscordNotifier(Instance instance)
    {
        _instance = instance;
    }

    public void RegisterJobs()
    {
        UnregisterJobs();

#if DEBUG
        var i = 0;
#endif
        foreach (var occurrence in _instance.CalendarSync.Calendar.GetOccurrences(DateTime.Now,
                     DateTime.Now + TimeSpan.FromDays(_instance.CalendarEntity.MaxDays)))
        {
            var evt = (CalendarEvent)occurrence.Source;
            var runAt = occurrence.Period.StartTime
                .Add(TimeSpan.FromSeconds(_instance.CalendarEntity.ReminderOffsetSeconds))
                .Value;
            var timestamp = occurrence.Period.StartTime.AsUtc.Subtract(DateTime.UnixEpoch).TotalSeconds;

#if DEBUG
            if (i == 0)
            {
                runAt = DateTime.Now + TimeSpan.FromSeconds(5);
                i++;
            }

            Console.WriteLine(runAt);
#endif
            // Skip already passed event
            if (DateTime.Now > runAt) continue;

            JobManager.AddJob(() => { Notify(timestamp); },
                schedule => schedule.WithName("discord-notify-" + _instance.CalendarEntity.Id + "-" + timestamp)
                    .ToRunOnceAt(runAt));
        }
    }

    public void UnregisterJobs()
    {
        foreach (var b in JobManager.AllSchedules.Where(s =>
                     s.Name.StartsWith("discord-notify-" + _instance.CalendarEntity.Id + "-")))
        {
            JobManager.RemoveJob(b.Name);
        }
    }

    public async void Notify(double timestamp)
    {
        var i = _instance.CalendarEntity.NextSentence % _instance.CalendarEntity.Sentences.Count;
        _instance.CalendarEntity.NextSentence = i;
        var str = _instance.CalendarEntity.Sentences[i];
        _instance.CalendarEntity.NextSentence = (i + 1) % _instance.CalendarEntity.Sentences.Count;

        str = _instance.CalendarEntity.AvailableMentions.Aggregate(str,
            (current, keyValuePair) => current.Replace(keyValuePair.Key, keyValuePair.Value)
        );
        str = new[] { 't', 'T', 'd', 'D', 'f', 'F', 'R' }.Aggregate(str,
            (current, format) => current.Replace($"<{format}>", $"<t:{timestamp}:{format}>")
        );
        Console.WriteLine("runner" + _instance.CalendarEntity.ReminderChannel);

        await new DiscordMessageBuilder()
            .WithAllowedMention(RoleMention.All)
            .WithAllowedMention(UserMention.All)
            .WithAllowedMention(EveryoneMention.All)
            .WithContent(str)
            .SendAsync(await GetChannel(_instance.CalendarEntity.ReminderChannel));
        _instance.Update();
        // _instance.ConfigurationManager.Save();
    }

    private async Task<DiscordChannel> GetChannel(ulong channel)
    {
        // todo: handle channel not existing
        return await _instance.DiscordManager.DiscordClient.GetChannelAsync(channel);
    }

    public void Dispose()
    {
        UnregisterJobs();
        GC.SuppressFinalize(this);
    }
}