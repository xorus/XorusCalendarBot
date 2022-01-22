using XorusCalendarBot;
using XorusCalendarBot.Calendar;
using XorusCalendarBot.Configuration;
using XorusCalendarBot.Database;
using XorusCalendarBot.Discord;
using XorusCalendarBot.Scheduler;

namespace XorusCalendarBot;

internal class Program
{
    static void Main(string[] args)
    {
        var config = new ConfigurationManager();

        var db = new DatabaseManager(config.EnvConfig);
        var discordManager = new DiscordManager(config);

        var schedulers = new List<SchedulerManager>();

        foreach (var configInstance in config.AppConfig.Instances)
        {
            var calendarSync = new CalendarSync(configInstance);
            var instance = new Instance(config, configInstance, calendarSync, discordManager);

            var sm = new SchedulerManager(instance);
            schedulers.Add(sm);
            sm.Start();

            calendarSync.Refresh();
            discordManager.InstanceDictionary.Add(configInstance.RegisterCommandsTo, instance);

            config.OnLoad += (_, _) => calendarSync.Refresh();
        }

        discordManager.Connect();
        AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
        {
            Console.WriteLine("Exit requested");
            foreach (var schedulerManager in schedulers) schedulerManager.Scheduler.Shutdown().Wait();
            discordManager.DisconnectSync();
            db.Dispose();
        };
        Thread.Sleep(Timeout.Infinite);
    }
}