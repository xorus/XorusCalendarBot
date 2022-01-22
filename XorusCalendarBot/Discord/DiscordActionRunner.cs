using DSharpPlus.Entities;
using Quartz;

namespace XorusCalendarBot.Discord;

// ReSharper disable once ClassNeverInstantiated.Global - instantiated by Scheduler
public class DiscordActionRunner : IJob
{
    private Instance _instance;

    public async Task Execute(IJobExecutionContext context)
    {
        var data = context.JobDetail.JobDataMap;
        var instanceId = data.GetIntValue("instanceId");
        var timestamp = data.GetDoubleValue("timestamp");

        _instance = (Instance)context.Scheduler.Context.Get("Instance" + instanceId);

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

        _instance.ConfigurationManager.Save();
    }

    private async Task<DiscordChannel> GetChannel(ulong channel)
    {
        // todo: handle channel not existing
        return await _instance.DiscordManager.DiscordClient.GetChannelAsync(channel);
    }
}