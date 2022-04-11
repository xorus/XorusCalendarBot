using FluentScheduler;
using Swan.DependencyInjection;
using XorusCalendarBot;
using XorusCalendarBot.Api;
using XorusCalendarBot.Database;
using XorusCalendarBot.Discord;

DiscordManager discord;
JobManager.Initialize();

var serviceContainer = new DependencyContainer();

serviceContainer.Register(new Env());
serviceContainer.Register(new DatabaseManager(serviceContainer));
discord = new DiscordManager(serviceContainer);
discord.Connect();
serviceContainer.Register(discord);

var web = new Web(serviceContainer);

var instanceDictionary = new InstanceDictionary(serviceContainer);
serviceContainer.Register(instanceDictionary);
instanceDictionary.Init();

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    Console.WriteLine("Exit requested");
    JobManager.RemoveAllJobs();
    instanceDictionary.Dispose();
    discord.DisconnectSync();
    web.Dispose();
    serviceContainer.Resolve<DatabaseManager>().Dispose();
};
Thread.Sleep(Timeout.Infinite);