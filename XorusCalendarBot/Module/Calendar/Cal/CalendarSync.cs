using FluentScheduler;
using Ical.Net.CalendarComponents;
using Swan.Logging;

namespace XorusCalendarBot.Module.Calendar.Cal;

public sealed class CalendarSync : IDisposable
{
    private const string Name = "CalendarSync";
    private readonly Instance _instance;
    private readonly string _refreshJobName;
    private bool _started;

    public CalendarSync(Instance instance)
    {
        _instance = instance;
        _refreshJobName = "refresh-calendar-" + CalendarEntity.Id;
    }

    private CalendarEntity CalendarEntity => _instance.CalendarEntity;

    private Ical.Net.Calendar Calendar { get; set; } = new();

    public void Dispose()
    {
        StopAutoRefresh();
    }

    public async Task Refresh()
    {
        if (CalendarEntity.CalendarUrl.Length == 0) return;

        "Refreshing calendar".Info(Name);
        using var http = new HttpClient();
        try
        {
            var downloadedCalendarStream = await http.GetStreamAsync(CalendarEntity.CalendarUrl);
            var downloadedCalendar = Ical.Net.Calendar.Load(downloadedCalendarStream);
            var o = downloadedCalendar
                .GetOccurrences(DateTime.Now, DateTime.Now + TimeSpan.FromDays(CalendarEntity.MaxDays))
                .Select(o => o.Source)
                .Cast<CalendarEvent>()
                .Where(e =>
                {
                    // if (e.ExceptionDates.Count > 0)
                    // {
                    //     Console.WriteLine("---");
                    //     foreach (var argExceptionDate in e.ExceptionDates)
                    //     {
                    //         if (e.Start.Date == argExceptionDate.)
                    //         {
                    //             Console.WriteLine("Exception date: " + argExceptionDate);
                    //         }
                    //         Console.WriteLine(argExceptionDate);
                    //     }
                    // }
                    return (e.Summary.ToLower().StartsWith(CalendarEntity.CalendarEventPrefix.ToLower())
                            || e.Categories.Contains("Discord Event"))
                           && e.IsActive;
                })
                .Distinct().ToList();


            var nextEvents = new Ical.Net.Calendar();
            nextEvents.Events.AddRange(o);
            Calendar = nextEvents;
            CalendarEntity.NextOccurrences = Calendar.GetOccurrences(
                    DateTime.Now, DateTime.Now + TimeSpan.FromDays(CalendarEntity.MaxDays)
                ).Select(occurrence =>
                {
                    // ignore events in exception dates
                    return occurrence.Source.ExceptionDates.Any(
                        pl => pl.Any(p => p.CollidesWith(occurrence.Period)))
                        ? null
                        : occurrence.CreateFromICal(_instance);
                })
                .Where(e => e != null)
                .OrderBy(x => x!.StartTime) // cannot be null thanks to the previous Where clause
                .ToList()!;
            CalendarEntity.LastRefresh = DateTime.Now.ToUniversalTime();

            for (var i = 0; i < CalendarEntity.NextOccurrences.Count; i++)
            {
                var oc = CalendarEntity.NextOccurrences[i];
                if (oc.IsForced) continue;
                CalendarEntity.NextOccurrences[i].Message = DiscordNotifier.GetNextMessage(CalendarEntity, i);
            }

            _instance.Update();
            Updated?.Invoke(this, EventArgs.Empty);
        }
        catch (HttpRequestException e)
        {
            ("Cannot refresh " + CalendarEntity.CalendarUrl + " " + e.Message).Error(Name);
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
}