using XorusCalendarBot.Database;
using XorusCalendarBot.Discord;

namespace XorusCalendarBot;

public class Instance : IDisposable
{
    public readonly CalendarSync CalendarSync;
    public readonly CalendarEntity CalendarEntity;
    public readonly DiscordManager DiscordManager;
    private readonly DatabaseManager _databaseManager;
    private readonly DiscordNotifier _discordNotifier;

    public Instance(CalendarEntity calendarEntity, CalendarSync calendarSync, DiscordManager discordManager,
        DatabaseManager databaseManager)
    {
        CalendarEntity = calendarEntity;
        CalendarSync = calendarSync;
        DiscordManager = discordManager;
        _databaseManager = databaseManager;
        _discordNotifier = new DiscordNotifier(this);

        calendarSync.StartAutoRefresh();
        discordManager.InstanceDictionary.Add(CalendarEntity.RegisterCommandsTo, this);
        calendarSync.Updated += (_, _) => _discordNotifier.RegisterJobs();
    }

    public void Refresh()
    {
        CalendarSync.Refresh();
    }

    public void Update()
    {
        _databaseManager.Update(CalendarEntity);
    }

    public void Dispose()
    {
        _discordNotifier.Dispose();
        CalendarSync.Dispose();
        GC.SuppressFinalize(this);
    }
}