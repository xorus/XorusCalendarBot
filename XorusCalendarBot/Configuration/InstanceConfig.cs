namespace XorusCalendarBot.Configuration;

public class InstanceConfig
{
    public string CalendarEventPrefix = "raid night";
    public string CalendarUrl = "https://cloud.xorus.fr/remote.php/dav/public-calendars/BSydaTDPBGEpjFNN?export";
    public int MaxDays = 14;

    public string NextDateMessage = "Le <t:{start}:d> de <t:{start}:t> à <t:{end}:t> :";
    public string NothingPlannedMessage = "Nothing is planned at the moment.";

    public ulong RegisterCommandsTo = 755091710501060688;
    public ulong ReminderChannel { get; set; } = 901178455935299584;

    public Dictionary<string, string> AvailableMentions { get; set; } = new()
    {
        { "@Raid", "<@&902999673906823239>" }
    };

    public int ReminderOffsetSeconds { get; set; } = -10800;

    public List<string> Sentences { get; set; } = new()
    {
        "Date formats: <d> <D> <t> <T> <f> <F> <R>",
        "Rappel pour @Raid a <t>, <R> en fait :).",
        "❤ @Raid ❤ <t> (<R>) ❤"
    };

    public int NextSentence { get; set; } = 0;
}