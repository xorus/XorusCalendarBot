using Swan.DependencyInjection;
using XorusCalendarBot.Database;
using XorusCalendarBot.Discord;
using XorusCalendarBot.Module.Calendar.Cal;

namespace XorusCalendarBot.Module.Calendar;

public class Instance : IDisposable
{
    private readonly DiscordNotifier _discordNotifier;
    public CalendarEntity CalendarEntity { get; private set; }
    private readonly CalendarSync _calendarSync;
    public readonly DependencyContainer Container;

    public Instance(DependencyContainer container, CalendarEntity calendarEntity)
    {
        Container = container;
        CalendarEntity = calendarEntity;
        _calendarSync = new CalendarSync(this);
        _discordNotifier = new DiscordNotifier(this);

        _calendarSync.StartAutoRefresh();
        // discordManager.InstanceDictionary.Add(CalendarEntity.GuildId, this);
        _calendarSync.Updated += (_, _) => _discordNotifier.RegisterJobs();
    }

    public void Replace(CalendarEntity calendarEntity)
    {
        CalendarEntity = calendarEntity;
        Refresh();
    }

    public async Task ReplaceAsync(CalendarEntity calendarEntity)
    {
        CalendarEntity = calendarEntity;
        await RefreshAsync();
    }

    public void Dispose()
    {
        _discordNotifier.Dispose();
        _calendarSync.Dispose();
        GC.SuppressFinalize(this);
    }

    private void Refresh()
    {
#pragma warning disable CS4014
        _calendarSync.Refresh();
#pragma warning restore CS4014
    }

    public async Task RefreshAsync()
    {
        await _calendarSync.Refresh();
    }

    public void Update()
    {
        Container.Resolve<CalendarModule>().Update(CalendarEntity);
    }
}