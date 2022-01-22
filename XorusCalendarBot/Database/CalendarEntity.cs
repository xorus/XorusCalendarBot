namespace XorusCalendarBot.Database;

public class CalendarEntity
{
    public int Id { get; set; }

    public string CalendarEventPrefix { get; set; } = "raid night";

    public string CalendarUrl { get; set; } =
        "https://cloud.xorus.fr/remote.php/dav/public-calendars/BSydaTDPBGEpjFNN?export";

    public int MaxDays { get; set; } = 14;

    public string NextDateMessage { get; set; } = "Le <t:{start}:d> de <t:{start}:t> à <t:{end}:t> :";
    public string NothingPlannedMessage { get; set; } = "Nothing is planned at the moment.";

    public ulong RegisterCommandsTo { get; set; } = 755091710501060688;

    public Dictionary<string, string> AvailableMentions { get; set; } = new()
    {
        { "@Raid", "<@&902999673906823239>" }
    };

    public ulong ReminderChannel { get; set; } = 770620035935764521;

    public int ReminderOffsetSeconds { get; set; } = -10800;

    public List<string> Sentences { get; set; } = new()
    {
        "Date formats: <d> <D> <t> <T> <f> <F> <R>",
        "Rappel pour @Raid a <t>, <R> en fait :).",
        "❤ @Raid ❤ <t> (<R>) ❤"
    };

    public int NextSentence { get; set; } = 0;
}