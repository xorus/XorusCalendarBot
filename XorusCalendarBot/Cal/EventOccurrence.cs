using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace XorusCalendarBot.Cal;

public class EventOccurrence
{
    public DateTime StartTime { get; init; }
    public DateTime NotifyTime { get; init; }
    public string? Summary { get; init; }
    public string? Description { get; init; }
    public string? ForcedMessage { get; init; }
}

public static class EventOccurenceBuilder
{
    private const string EventCommandPrefix = "!event ";
    private const string EventMessageCommandPrefix = "!event-message ";

    public static EventOccurrence CreateFromICal(this Occurrence occurrence, Instance instance)
    {
        var source = ((CalendarEvent)occurrence.Source);
        var summary = source.Summary;
        if (summary != null && summary.StartsWith(EventCommandPrefix))
            summary = summary[EventCommandPrefix.Length..];
        var description = source.Description;

        return new EventOccurrence()
        {
            StartTime = occurrence.Period.StartTime.Value.ToUniversalTime(),
            NotifyTime = occurrence.Period.StartTime
                .Add(TimeSpan.FromSeconds(instance.CalendarEntity.ReminderOffsetSeconds))
                .Value,
            Summary = summary,
            Description = description,
            ForcedMessage = description != null && description.StartsWith(EventMessageCommandPrefix)
                ? description[EventMessageCommandPrefix.Length..]
                : null
        };
    }
}