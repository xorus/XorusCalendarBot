using System.Collections.Specialized;
using Ical.Net.CalendarComponents;
using Quartz;
using Quartz.Impl.Matchers;
using XorusDiscordBot;

namespace XorusCalendarBot;

public class SchedulerManager
{
    private readonly Instance Instance;
    private readonly int _id;

    public IScheduler Scheduler { get; private set; } = null!;

    public SchedulerManager(Instance instance, int id)
    {
        Instance = instance;
        _id = id;
    }

    public async void Start()
    {
        Scheduler = await SchedulerBuilder.Create(new NameValueCollection())
            .UseDefaultThreadPool(x => x.MaxConcurrency = 1)
            .BuildScheduler();
        await Scheduler.Start();

        Scheduler.Context.Put("Instance" + _id, Instance);
        Instance.CalendarSync.Updated += (_, _) => Repopulate();
    }

    private async void Repopulate()
    {
        Console.WriteLine("Calendar refreshed!" + Instance.CalendarSync.Calendar);

        // remove old jobs
        var refreshJob = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("global" + _id));
        await Scheduler.DeleteJobs(refreshJob);
        var jobs = await Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("events" + _id));
        await Scheduler.DeleteJobs(jobs);

        var rJob = JobBuilder.Create<RefreshActionRunner>()
            .WithIdentity("refresh", "global" + _id)
            .UsingJobData("instanceId", _id).Build();
        var rTrigger = TriggerBuilder.Create()
            .WithIdentity("refresh", "global" + _id)
            .StartAt(DateTimeOffset.Now + TimeSpan.FromHours(5))
            .ForJob(rJob).Build();
        await Scheduler.ScheduleJob(rJob, rTrigger);

        // var i = 0;
        foreach (var occurrence in Instance.CalendarSync.Calendar.GetOccurrences(DateTime.Now,
                     DateTime.Now + TimeSpan.FromDays(Instance.Config.MaxDays)))
        {
            var evt = (CalendarEvent)occurrence.Source;
            var job = JobBuilder.Create<DiscordActionRunner>()
                .WithIdentity("event" + evt.Uid + occurrence.Period.StartTime, "events" + _id)
                .UsingJobData("instanceId", _id)
                .UsingJobData("timestamp", occurrence.Period.StartTime.AsUtc.Subtract(DateTime.UnixEpoch).TotalSeconds)
                .Build();

            var runAt = occurrence.Period.StartTime.Add(TimeSpan.FromSeconds(Instance.Config.ReminderOffsetSeconds))
                .Value;
            Console.WriteLine(runAt);
            // Skip already passed event
            if (DateTime.Now > runAt) continue;
            //
            // if (i == 0)
            // {
            //     runAt = DateTime.Now + TimeSpan.FromSeconds(5);
            //     i++;
            // }

            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger" + evt.Uid + occurrence.Period.StartTime, "events" + _id)
                .StartAt(runAt)
                .ForJob(job)
                .Build();

            await Scheduler.ScheduleJob(job, trigger);
        }
    }
}