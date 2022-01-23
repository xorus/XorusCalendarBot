using FluentScheduler;
using XorusCalendarBot;
using XorusCalendarBot.Api;
using XorusCalendarBot.Database;
using XorusCalendarBot.Discord;

Dictionary<Guid, Instance> instances = new();
DiscordManager discord;
DatabaseManager db;

var env = new Env();

JobManager.Initialize();
db = new DatabaseManager(env);
discord = new DiscordManager(env);
discord.Connect();

var web = new Web(env, db);

// to re-run when changing the collection
void CreateInstances(IEnumerable<CalendarEntity> calendarEntities)
{
    foreach (var calendarEntity in calendarEntities)
    {
        // todo: update instead or destroying
        if (instances.ContainsKey(calendarEntity.Id))
        {
            var id = calendarEntity.Id;
            instances[id].Dispose();
            instances.Remove(id);
        }

        var instance = new Instance(calendarEntity, new CalendarSync(calendarEntity), discord, db);
        instances.Add(instance.CalendarEntity.Id, instance);
    }
}

CreateInstances(db.CalendarEntities);

AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
{
    Console.WriteLine("Exit requested");
    foreach (var instance in instances) instance.Value.Dispose();
    JobManager.RemoveAllJobs();
    discord.DisconnectSync();
    web.Dispose();
    db.Dispose();
};
Thread.Sleep(Timeout.Infinite);