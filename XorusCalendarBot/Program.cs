using XorusCalendarBot;

var config = new ConfigurationManager();
var discordManager = new DiscordManager(config);

var schedulers = new List<SchedulerManager>();

var i = 0;
foreach (var configInstance in config.AppConfig.Instances)
{
    var calendarSync = new CalendarSync(configInstance);
    var instance = new Instance(config, configInstance, calendarSync, discordManager);


    var sm = new SchedulerManager(instance, i++);
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
    foreach (var schedulerManager in schedulers)
    {
        schedulerManager.Scheduler.Shutdown().Wait();
    }

    discordManager.DisconnectSync();
};
Thread.Sleep(Timeout.Infinite);