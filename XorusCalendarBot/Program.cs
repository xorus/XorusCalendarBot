using FluentScheduler;
using Swan.DependencyInjection;
using XorusCalendarBot;
using XorusCalendarBot.Api;
using XorusCalendarBot.Database;
using XorusCalendarBot.Discord;
using XorusCalendarBot.Module.Base;
using XorusCalendarBot.Module.Calendar;
using XorusCalendarBot.Module.Soundboard;

DiscordManager discord;
JobManager.Initialize();

var serviceContainer = new DependencyContainer();

serviceContainer.Register(new Env());
serviceContainer.Register(new DatabaseManager(serviceContainer));
discord = new DiscordManager(serviceContainer);
discord.Connect();
serviceContainer.Register(discord);

var modules = new List<IModule>
{
    new CalendarModule(serviceContainer),
    new SoundboardModule(serviceContainer)
};
var web = new Web(serviceContainer, modules);

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    Console.WriteLine("Exit requested");
    JobManager.RemoveAllJobs();
    foreach (var module in modules) module.Dispose();
    discord.DisconnectSync();
    web.Dispose();
    serviceContainer.Resolve<DatabaseManager>().Dispose();
};
Thread.Sleep(Timeout.Infinite);