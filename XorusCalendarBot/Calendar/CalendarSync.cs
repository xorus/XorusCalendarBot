﻿using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using XorusCalendarBot.Configuration;

namespace XorusCalendarBot.Calendar;

public class CalendarSync
{
    private readonly InstanceConfig _config;

    public CalendarSync(InstanceConfig config)
    {
        _config = config;
    }

    public Ical.Net.Calendar Calendar { get; private set; } = new();

    public async void Refresh()
    {
        Console.WriteLine("Refreshing calendar");
        using var http = new HttpClient();
        try
        {
            var downloadedCalendarStream = await http.GetStreamAsync(_config.CalendarUrl);
            var downloadedCalendar = Ical.Net.Calendar.Load(downloadedCalendarStream);
            var o = downloadedCalendar.GetOccurrences(DateTime.Now, DateTime.Now + TimeSpan.FromDays(_config.MaxDays))
                .Select(o => o.Source)
                .Cast<CalendarEvent>()
                .Where(e => e.Summary.ToLower().StartsWith(_config.CalendarEventPrefix.ToLower()))
                .Distinct().ToList();

            var nextEvents = new Ical.Net.Calendar();
            nextEvents.Events.AddRange(o);
            Calendar = nextEvents;
            OnUpdate();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Cannot refresh " + _config.CalendarUrl + " " + e.Message);
        }
    }

    public event EventHandler? Updated;

    protected virtual void OnUpdate()
    {
        Updated?.Invoke(this, EventArgs.Empty);
    }

    public static double ToUnixTimestamp(IDateTime date)
    {
        return date.AsUtc.Subtract(DateTime.UnixEpoch).TotalSeconds;
    }
}