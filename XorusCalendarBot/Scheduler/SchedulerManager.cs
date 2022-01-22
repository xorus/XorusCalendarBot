using System.Collections.Specialized;
using Ical.Net.CalendarComponents;
using Quartz;
using Quartz.Impl.Matchers;
using XorusCalendarBot.Discord;

namespace XorusCalendarBot.Scheduler;

public class SchedulerManager
{
    private readonly Instance _instance;

    public SchedulerManager(Instance instance)
    {
        _instance = instance;
    }

    public IScheduler Scheduler { get; set; } = null!;

    public async void Start()
    {
        Scheduler = await SchedulerBuilder.Create(new NameValueCollection())
            .UseDefaultThreadPool(x => x.MaxConcurrency = 1)
            .BuildScheduler();
        await Scheduler.Start();

        Scheduler.Context.Put("Instance" + _instance.CalendarEntity.Id, _instance);
        _instance.CalendarSync.Updated += (_, _) => Repopulate();
    }

    private async void Repopulate()
    {
        Console.WriteLine("Calendar refreshed!" + _instance.CalendarSync.Calendar);

        // remove old jobs
        var refreshJob = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("global" + _instance.CalendarEntity.Id));
        await Scheduler.DeleteJobs(refreshJob);
        var jobs = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("events" + _instance.CalendarEntity.Id));
        await Scheduler.DeleteJobs(jobs);

        var rJob = JobBuilder.Create<RefreshActionRunner>()
            .WithIdentity("refresh", "global" + _instance.CalendarEntity.Id)
            .UsingJobData("instanceId", _instance.CalendarEntity.Id).Build();
        var rTrigger = TriggerBuilder.Create()
            .WithIdentity("refresh", "global" + _instance.CalendarEntity.Id)
            // .StartAt(DateTimeOffset.Now + TimeSpan.FromHours(5))
            .StartAt(DateTimeOffset.Now + TimeSpan.FromSeconds(10))
            .ForJob(rJob).Build();
        await Scheduler.ScheduleJob(rJob, rTrigger);
        Console.WriteLine(rTrigger.StartTimeUtc);

#if DEBUG
        var i = 0;
#endif
        foreach (var occurrence in _instance.CalendarSync.Calendar.GetOccurrences(DateTime.Now,
                     DateTime.Now + TimeSpan.FromDays(_instance.CalendarEntity.MaxDays)))
        {
            var evt = (CalendarEvent)occurrence.Source;
            var job = JobBuilder.Create<DiscordActionRunner>()
                .WithIdentity("event" + evt.Uid + occurrence.Period.StartTime, "events" + _instance.CalendarEntity.Id)
                .UsingJobData("instanceId", _instance.CalendarEntity.Id)
                .UsingJobData("timestamp", occurrence.Period.StartTime.AsUtc.Subtract(DateTime.UnixEpoch).TotalSeconds)
                .Build();

            var runAt = occurrence.Period.StartTime.Add(TimeSpan.FromSeconds(_instance.CalendarEntity.ReminderOffsetSeconds))
                .Value;
#if DEBUG
            if (i == 0)
            {
                runAt = DateTime.Now + TimeSpan.FromSeconds(5);
                i++;
            }
            Console.WriteLine(runAt);
#endif
            // Skip already passed event
            if (DateTime.Now > runAt) continue;


            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger" + evt.Uid + occurrence.Period.StartTime, "events" + _instance.CalendarEntity.Id)
                .StartAt(runAt)
                .ForJob(job)
                .Build();

            await Scheduler.ScheduleJob(job, trigger);
        }
    }
}