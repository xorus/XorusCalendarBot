using XorusCalendarBot.Discord;

namespace XorusCalendarBot.Module.Calendar;

// https://github.com/DSharpPlus/DSharpPlus/tree/master/DSharpPlus.SlashCommands
// ReSharper disable once ClassNeverInstantiated.Global
public class SlashCommands
{
    // set via Dependency Injection
    public DiscordManager? DiscordManager { private get; set; }

    // private static string ReplaceThings(string str, Dictionary<string, string> placeholders, bool escape = true)
    // {
    //     if (escape) str = Regex.Replace(str.Trim(), @"([|\\*])", @"\$1");
    //     return placeholders.Aggregate(str,
    //         (current, keyValuePair) => current.Replace(keyValuePair.Key, keyValuePair.Value));
    // }

    // private static string FormatEvent(string nextDateMessage, EventOccurrence next, bool allowDescription)
    // {
    //     var messageText = "";
    //     var placeholders = new Dictionary<string, string>
    //     {
    //         { @"{start}", next.StartTime.ToUnixTimestamp().ToString(CultureInfo.InvariantCulture) },
    //         { @"{end}", next.EndTime.ToUnixTimestamp().ToString(CultureInfo.InvariantCulture) }
    //     };
    //     messageText += ReplaceThings(nextDateMessage, placeholders);
    //
    //     var description = "";
    //     var hasDescription = false;
    //     if (allowDescription && next.Description != null)
    //     {
    //         description = ReplaceThings(next.Description, placeholders).Trim();
    //         hasDescription = description.Length > 0;
    //     }
    //     // if (hasDescription) messageText += "\n";
    //     // else messageText += " ";
    //
    //     messageText += $" **{ReplaceThings(next.Summary ?? "Event", placeholders, false)}**";
    //
    //     if (hasDescription)
    //         messageText += "\n> " + string.Join("\n> ",
    //             description.Split("\n").Select(x => x.Trim()).Where(x => x.Length > 0));
    //
    //     return messageText;
    // }

    // // ReSharper disable once UnusedMember.Global
    // [SlashCommand("next", "Displays the next event")]
    // public async Task NextEvent(InteractionContext ctx,
    //     [Option("count", "Number of events to show, defaults to one.")]
    //     long? count = null)
    // {
    //     Debug.Assert(DiscordManager != null, nameof(DiscordManager) + " != null");
    //     if (!DiscordManager.InstanceDictionary.ContainsKey(ctx.Guild.Id.ToString()))
    //     {
    //         Console.WriteLine("Cannot find instance for guild " + ctx.Guild.Id);
    //         return;
    //     }
    //
    //     var instances = DiscordManager.InstanceDictionary[ctx.Guild.Id.ToString()];
    //
    //     var occurrences = new List<EventOccurrence>();
    //     var nothingPlannedMessage = "";
    //     var nextDateMessage = "";
    //     foreach (var instance in instances)
    //     {
    //         nothingPlannedMessage = instance.CalendarEntity.NothingPlannedMessage;
    //         nextDateMessage = instance.CalendarEntity.NextDateMessage;
    //         occurrences.AddRange(instance.CalendarEntity.NextOccurrences
    //             .Take((int)(count ?? 1))
    //             .ToImmutableArray());
    //     }
    //
    //     var messageText = occurrences.Count == 0
    //         ? $"\n{nothingPlannedMessage}"
    //         : occurrences.Aggregate("", (current, occurrence) =>
    //             current + FormatEvent(nextDateMessage, occurrence, occurrences.Count == 1) + "\n");
    //
    //     await ctx.CreateResponseAsync(
    //         InteractionResponseType.ChannelMessageWithSource,
    //         new DiscordInteractionResponseBuilder().WithContent(messageText)
    //     );
    // }

//     // ReSharper disable once UnusedMember.Global
//     [SlashCommand("reload", "force reload calendar")]
//     public async Task Reload(InteractionContext ctx)
//     {
//         Debug.Assert(DiscordManager != null, nameof(DiscordManager) + " != null");
//         if (!DiscordManager.InstanceDictionary.ContainsKey(ctx.Guild.Id.ToString()))
//         {
//             Console.WriteLine("Cannot find instance for guild " + ctx.Guild.Id);
//             return;
//         }
//
//         await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
//         var instances = DiscordManager.InstanceDictionary[ctx.Guild.Id.ToString()];
//         foreach (var instance in instances) await instance.CalendarSync.Refresh();
//
//         await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Job done!"));
//         Thread.Sleep(1000);
//         await ctx.DeleteResponseAsync();
//     }
}