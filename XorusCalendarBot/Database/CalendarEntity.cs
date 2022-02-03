using XorusCalendarBot.Cal;

namespace XorusCalendarBot.Database;

public class CalendarEntity
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public string CalendarEventPrefix { get; set; } = "raid night";

    public string CalendarUrl { get; set; } = "";

    public int MaxDays { get; set; } = 14;

    public string NextDateMessage { get; set; } = "<t:{start}:d> <t:{start}:t> - <t:{end}:t> :";
    public string NothingPlannedMessage { get; set; } = "Nothing is planned at the moment.";

    public string GuildId { get; set; } = "";

    public string ReminderChannel { get; set; } = "";

    public int ReminderOffsetSeconds { get; set; } = -10800;

    public List<string> Sentences { get; set; } = new();
    public int NextSentence { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<EventOccurrence> NextOccurrences { get; set; } = new List<EventOccurrence>();

    public DateTime? LastRefresh { get; set; }

    public static CalendarEntity CreateDefault()
    {
        return new CalendarEntity
        {
            Sentences = new List<string>
            {
                "Date formats: <d> <D> <t> <T> <f> <F> <R>",
                "Hey @Role, the thing is at <t>!",
            }
        };
    }
}