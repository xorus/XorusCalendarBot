using FluentScheduler;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using XorusCalendarBot.Database;

namespace XorusCalendarBot.Cal;

public sealed class CalendarSync : IDisposable
{
    private readonly string _refreshJobName;
    private readonly Instance _instance;
    private CalendarEntity CalendarEntity => _instance.CalendarEntity;
    private bool _started;

    public CalendarSync(Instance instance)
    {
        _instance = instance;
        _refreshJobName = "refresh-calendar-" + CalendarEntity.Id;
    }

    public Calendar Calendar { get; private set; } = new();

    public void Dispose()
    {
        StopAutoRefresh();
    }

    public async void Refresh()
    {
        Console.WriteLine("Refreshing calendar");
        using var http = new HttpClient();
        try
        {
            var downloadedCalendarStream = await http.GetStreamAsync(CalendarEntity.CalendarUrl);
            var downloadedCalendar = Calendar.Load(downloadedCalendarStream);
            var o = downloadedCalendar
                .GetOccurrences(DateTime.Now, DateTime.Now + TimeSpan.FromDays(CalendarEntity.MaxDays))
                .Select(o => o.Source)
                .Cast<CalendarEvent>()
                .Where(e => e.Summary.ToLower().StartsWith(CalendarEntity.CalendarEventPrefix.ToLower())
                            || e.Categories.Contains("Discord Event"))
                .Distinct().ToList();

            var nextEvents = new Calendar();
            nextEvents.Events.AddRange(o);
            Calendar = nextEvents;

            Console.WriteLine("refresh");

            CalendarEntity.NextOccurrences = Calendar.GetOccurrences(
                DateTime.Now, DateTime.Now + TimeSpan.FromDays(CalendarEntity.MaxDays)
            ).Select(occurrence => occurrence.CreateFromICal(_instance)).ToList();
            CalendarEntity.LastRefresh = DateTime.Now.ToUniversalTime();
            _instance.Update();

            OnUpdate();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Cannot refresh " + CalendarEntity.CalendarUrl + " " + e.Message);
        }
    }

    public void StartAutoRefresh()
    {
        if (_started) return;
        _started = true;
        Refresh();
        JobManager.AddJob(Refresh, schedule => schedule.WithName(_refreshJobName).ToRunEvery(5).Hours());
    }

    private void StopAutoRefresh()
    {
        JobManager.RemoveJob(_refreshJobName);
        _started = false;
    }

    public event EventHandler? Updated;

    private void OnUpdate()
    {
        Updated?.Invoke(this, EventArgs.Empty);
    }

    public static double ToUnixTimestamp(IDateTime date)
    {
        return date.AsUtc.Subtract(DateTime.UnixEpoch).TotalSeconds;
    }
}