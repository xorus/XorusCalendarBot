using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using XorusCalendarBot.Calendar;

namespace XorusCalendarBot.Discord;

// https://github.com/DSharpPlus/DSharpPlus/tree/master/DSharpPlus.SlashCommands
public class SlashCommands : ApplicationCommandModule
{
    // public Instance Instance { private get; set; }

    public DiscordManager DiscordManager { private get; set; }

    private string ReplaceThings(string str, Dictionary<string, string> placeholders, bool escape = true)
    {
        if (escape) str = Regex.Replace(str.Trim(), @"([|\\*])", @"\$1");
        return placeholders.Aggregate(str,
            (current, keyValuePair) => current.Replace(keyValuePair.Key, keyValuePair.Value));
    }

    private string FormatEvent(string nextDateMessage, Occurrence next, bool allowDescription)
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

    [SlashCommand("next", "Displays the next event")]
    public async Task NextEvent(InteractionContext ctx,
        [Option("count", "Number of events to show, defaults to one.")]
        long? count = null)
    {
        if (!DiscordManager.InstanceDictionary.ContainsKey(ctx.Guild.Id))
        {
            Console.WriteLine("Cannot find instance for guild " + ctx.Guild.Id);
            return;
        }

        var instance = DiscordManager.InstanceDictionary[ctx.Guild.Id];

        var occurrences = instance.CalendarSync.Calendar
            .GetOccurrences(DateTime.Now, DateTime.Now + TimeSpan.FromDays(instance.Config.MaxDays))
            .Take((int)(count ?? 1))
            .OrderBy(x => x.Period.StartTime.Date)
            .ToImmutableArray();

        var messageText = occurrences.Length == 0
            ? $"\n{instance.Config.NothingPlannedMessage}"
            : Enumerable.Aggregate(occurrences, "",
                (current, occurrence) =>
                    current + FormatEvent(instance.Config.NextDateMessage, occurrence, occurrences.Length == 1) + "\n");

        await ctx.CreateResponseAsync(
            InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(messageText)
        );
    }

    [SlashCommand("reload", "force reload calendar")]
    public async Task Reload(InteractionContext ctx)
    {
        if (!DiscordManager.InstanceDictionary.ContainsKey(ctx.Guild.Id))
        {
            Console.WriteLine("Cannot find instance for guild " + ctx.Guild.Id);
            return;
        }

        var instance = DiscordManager.InstanceDictionary[ctx.Guild.Id];

        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        instance.CalendarSync.Refresh();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Job done!"));
        Thread.Sleep(1000);
        await ctx.DeleteResponseAsync();
    }
}