using Ical.Net.DataTypes;
using LiteDB;
using XorusCalendarBot.Cal;

namespace XorusCalendarBot.Database;

public class CalendarEntity
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public string CalendarEventPrefix { get; set; } = "raid night";

    public string CalendarUrl { get; set; } =
        "https://cloud.xorus.fr/remote.php/dav/public-calendars/BSydaTDPBGEpjFNN?export";

    public int MaxDays { get; set; } = 14;

    public string NextDateMessage { get; set; } = "Le <t:{start}:d> de <t:{start}:t> à <t:{end}:t> :";
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
                "Rappel pour @Raid a <t>, <R> en fait :).",
                "❤ @Raid ❤ <t> (<R>) ❤"
            }
        };
    }
}