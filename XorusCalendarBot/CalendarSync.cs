using FluentScheduler;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using XorusCalendarBot.Database;

namespace XorusCalendarBot;

public sealed class CalendarSync : IDisposable
{
    private readonly CalendarEntity _calendarEntity;
    private readonly string _refreshJobName;
    private bool _started = false;

    public CalendarSync(CalendarEntity calendarEntity)
    {
        _calendarEntity = calendarEntity;
        _refreshJobName = "refresh-calendar-" + _calendarEntity.Id;
    }

    public Ical.Net.Calendar Calendar { get; private set; } = new();

    public async void Refresh()
    {
        Console.WriteLine("Refreshing calendar");
        using var http = new HttpClient();
        try
        {
            var downloadedCalendarStream = await http.GetStreamAsync(_calendarEntity.CalendarUrl);
            var downloadedCalendar = Ical.Net.Calendar.Load(downloadedCalendarStream);
            var o = downloadedCalendar
                .GetOccurrences(DateTime.Now, DateTime.Now + TimeSpan.FromDays(_calendarEntity.MaxDays))
                .Select(o => o.Source)
                .Cast<CalendarEvent>()
                .Where(e => e.Summary.ToLower().StartsWith(_calendarEntity.CalendarEventPrefix.ToLower()))
                .Distinct().ToList();

            var nextEvents = new Ical.Net.Calendar();
            nextEvents.Events.AddRange(o);
            Calendar = nextEvents;
            OnUpdate();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Cannot refresh " + _calendarEntity.CalendarUrl + " " + e.Message);
        }
    }

    public void StartAutoRefresh()
    {
        if (_started) return;
        _started = true;
        JobManager.AddJob(Refresh, schedule => schedule.WithName(_refreshJobName).ToRunEvery(30).Seconds());
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

    public void Dispose()
    {
        StopAutoRefresh();
    }
}