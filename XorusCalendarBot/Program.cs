using FluentScheduler;
using Swan.DependencyInjection;
using XorusCalendarBot;
using XorusCalendarBot.Api;
using XorusCalendarBot.Database;
using XorusCalendarBot.Discord;

DiscordManager discord;
DatabaseManager db;

JobManager.Initialize();

var serviceContainer = new DependencyContainer();

var env = new Env();
serviceContainer.Register(env);

db = new DatabaseManager(serviceContainer);
discord = new DiscordManager(env);
discord.Connect();
serviceContainer.Register(db);
serviceContainer.Register(discord);

var web = new Web(serviceContainer);

var instanceDictionary = new InstanceDictionary(serviceContainer);
serviceContainer.Register(instanceDictionary);
instanceDictionary.Init();

AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
{
    Console.WriteLine("Exit requested");
    JobManager.RemoveAllJobs();
    instanceDictionary.Dispose();
    discord.DisconnectSync();
    web.Dispose();
    db.Dispose();
};
Thread.Sleep(Timeout.Infinite);