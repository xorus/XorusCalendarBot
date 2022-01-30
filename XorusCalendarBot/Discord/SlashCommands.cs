using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace XorusCalendarBot.Discord;

// https://github.com/DSharpPlus/DSharpPlus/tree/master/DSharpPlus.SlashCommands
// ReSharper disable once ClassNeverInstantiated.Global
public class SlashCommands : ApplicationCommandModule
{
    // set via Dependency Injection
    public DiscordManager? DiscordManager { private get; set; }

    private static string ReplaceThings(string str, Dictionary<string, string> placeholders, bool escape = true)
    {
        if (escape) str = Regex.Replace(str.Trim(), @"([|\\*])", @"\$1");
        return placeholders.Aggregate(str,
            (current, keyValuePair) => current.Replace(keyValuePair.Key, keyValuePair.Value));
    }

    private static string FormatEvent(string nextDateMessage, Occurrence next, bool allowDescription)
    {
        var messageText = "";
        var placeholders = new Dictionary<string, string>
        {
            { @"{start}", CalendarSync.ToUnixTimestamp(next.Period.StartTime).ToString(CultureInfo.InvariantCulture) },
            { @"{end}", CalendarSync.ToUnixTimestamp(next.Period.EndTime).ToString(CultureInfo.InvariantCulture) }
        };
        messageText += ReplaceThings(nextDateMessage, placeholders);

        var calendarEvent = (CalendarEvent)next.Source;

        var description = "";
        var hasDescription = false;
        if (allowDescription)
        {
            description = ReplaceThings(calendarEvent.Description, placeholders).Trim();
            hasDescription = description.Length > 0;
        }
        // if (hasDescription) messageText += "\n";
        // else messageText += " ";

        messageText += $" **{ReplaceThings(calendarEvent.Summary, placeholders, false)}**";

        if (hasDescription)
            messageText += "\n> " + string.Join("\n> ",
                description.Split("\n").Select(x => x.Trim()).Where(x => x.Length > 0));

        return messageText;
    }

    // ReSharper disable once UnusedMember.Global
    [SlashCommand("next", "Displays the next event")]
    public async Task NextEvent(InteractionContext ctx,
        [Option("count", "Number of events to show, defaults to one.")]
        long? count = null)
    {
        Debug.Assert(DiscordManager != null, nameof(DiscordManager) + " != null");
        if (!DiscordManager.InstanceDictionary.ContainsKey(ctx.Guild.Id.ToString()))
        {
            Console.WriteLine("Cannot find instance for guild " + ctx.Guild.Id);
            return;
        }

        var instances = DiscordManager.InstanceDictionary[ctx.Guild.Id.ToString()];

        var occurrences = new List<Occurrence>();
        var nothingPlannedMessage = "";
        var nextDateMessage = "";
        foreach (var instance in instances)
        {
            nothingPlannedMessage = instance.CalendarEntity.NothingPlannedMessage;
            nextDateMessage = instance.CalendarEntity.NextDateMessage;
            occurrences.AddRange(instance.CalendarSync.Calendar
                .GetOccurrences(DateTime.Now, DateTime.Now + TimeSpan.FromDays(instance.CalendarEntity.MaxDays))
                .Take((int)(count ?? 1))
                .OrderBy(x => x.Period.StartTime.Date)
                .ToImmutableArray());
        }

        var messageText = occurrences.Count == 0
            ? $"\n{nothingPlannedMessage}"
            : occurrences.Aggregate("", (current, occurrence) =>
                current + FormatEvent(nextDateMessage, occurrence, occurrences.Count == 1) + "\n");

        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(messageText)
        );
    }

    // ReSharper disable once UnusedMember.Global
    [SlashCommand("reload", "force reload calendar")]
    public async Task Reload(InteractionContext ctx)
    {
        Debug.Assert(DiscordManager != null, nameof(DiscordManager) + " != null");
        if (!DiscordManager.InstanceDictionary.ContainsKey(ctx.Guild.Id.ToString()))
        {
            Console.WriteLine("Cannot find instance for guild " + ctx.Guild.Id);
            return;
        }

        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        var instances = DiscordManager.InstanceDictionary[ctx.Guild.Id.ToString()];
        foreach (var instance in instances) instance.CalendarSync.Refresh();

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Job done!"));
        Thread.Sleep(1000);
        await ctx.DeleteResponseAsync();
    }
}