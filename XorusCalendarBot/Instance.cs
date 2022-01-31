using Swan.DependencyInjection;
using XorusCalendarBot.Cal;
using XorusCalendarBot.Database;
using XorusCalendarBot.Discord;

namespace XorusCalendarBot;

public class Instance : IDisposable
{
    private readonly DiscordNotifier _discordNotifier;
    public CalendarEntity CalendarEntity { get; private set; }
    public readonly CalendarSync CalendarSync;
    public readonly DependencyContainer Container;

    public Instance(DependencyContainer container, CalendarEntity calendarEntity)
    {
        Container = container;
        CalendarEntity = calendarEntity;
        CalendarSync = new CalendarSync(this);
        _discordNotifier = new DiscordNotifier(this);

        CalendarSync.StartAutoRefresh();
        // discordManager.InstanceDictionary.Add(CalendarEntity.GuildId, this);
        CalendarSync.Updated += (_, _) => _discordNotifier.RegisterJobs();
    }

    public void Replace(CalendarEntity calendarEntity)
    {
        CalendarEntity = calendarEntity;
        Refresh();
    }

    public void Dispose()
    {
        _discordNotifier.Dispose();
        CalendarSync.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Refresh()
    {
#pragma warning disable CS4014
        CalendarSync.Refresh();
#pragma warning restore CS4014
    }

    public async Task RefreshAsync()
    {
        await CalendarSync.Refresh();
    }

    public void Update()
    {
        Container.Resolve<DatabaseManager>().Update(CalendarEntity);
    }
}