using XorusCalendarBot.Database;
using XorusCalendarBot.Discord;

namespace XorusCalendarBot;

public class Instance : IDisposable
{
    private readonly DatabaseManager _databaseManager;
    private readonly DiscordNotifier _discordNotifier;
    public readonly CalendarEntity CalendarEntity;
    public readonly CalendarSync CalendarSync;
    public readonly DiscordManager DiscordManager;

    public Instance(CalendarEntity calendarEntity, CalendarSync calendarSync, DiscordManager discordManager,
        DatabaseManager databaseManager)
    {
        CalendarEntity = calendarEntity;
        CalendarSync = calendarSync;
        DiscordManager = discordManager;
        _databaseManager = databaseManager;
        _discordNotifier = new DiscordNotifier(this, discordManager);

        calendarSync.StartAutoRefresh();
        // discordManager.InstanceDictionary.Add(CalendarEntity.GuildId, this);
        calendarSync.Updated += (_, _) => _discordNotifier.RegisterJobs();
    }

    public void Dispose()
    {
        _discordNotifier.Dispose();
        CalendarSync.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Refresh()
    {
        CalendarSync.Refresh();
    }

    public void Update()
    {
        _databaseManager.Update(CalendarEntity);
    }
}