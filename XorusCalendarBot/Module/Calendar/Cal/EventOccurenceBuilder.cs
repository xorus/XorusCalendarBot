using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;

namespace XorusCalendarBot.Module.Calendar.Cal;

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
        var forcedMessage = description != null && description.StartsWith(EventMessageCommandPrefix)
            ? description[EventMessageCommandPrefix.Length..]
            : null;
        
        return new EventOccurrence()
        {
            StartTime = occurrence.Period.StartTime.Value.ToUniversalTime(),
            EndTime = occurrence.Period.EndTime.Value.ToUniversalTime(),
            NotifyTime = occurrence.Period.StartTime
                .Add(TimeSpan.FromSeconds(instance.CalendarEntity.ReminderOffsetSeconds))
                .Value,
            Summary = summary,
            Description = description,
            Message = forcedMessage,
            IsForced = forcedMessage != null
        };
    }
}