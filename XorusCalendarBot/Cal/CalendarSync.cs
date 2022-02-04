using FluentScheduler;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using XorusCalendarBot.Database;
using XorusCalendarBot.Discord;

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

    private Calendar Calendar { get; set; } = new();

    public void Dispose()
    {
        StopAutoRefresh();
    }

    public async Task Refresh()
    {
        if (CalendarEntity.CalendarUrl.Length == 0) return;
        
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
            CalendarEntity.NextOccurrences = Calendar.GetOccurrences(
                    DateTime.Now, DateTime.Now + TimeSpan.FromDays(CalendarEntity.MaxDays)
                ).Select(occurrence => occurrence.CreateFromICal(_instance))
                .OrderBy(x => x.StartTime)
                .ToList();
            CalendarEntity.LastRefresh = DateTime.Now.ToUniversalTime();

            for (var i = 0; i < CalendarEntity.NextOccurrences.Count; i++)
            {
                var oc = CalendarEntity.NextOccurrences[i];
                if (oc.IsForced) continue;
                CalendarEntity.NextOccurrences[i].Message = DiscordNotifier.GetNextMessage(CalendarEntity, i);
            }

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
#pragma warning disable CS4014
        Refresh();
        JobManager.AddJob(() => Refresh(), schedule => schedule.WithName(_refreshJobName).ToRunEvery(5).Hours());
#pragma warning restore CS4014
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
}