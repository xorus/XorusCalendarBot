using Quartz;

namespace XorusCalendarBot;

// ReSharper disable once ClassNeverInstantiated.Global
public class RefreshActionRunner : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        var data = context.JobDetail.JobDataMap;
        var instanceId = data.GetIntValue("instanceId");
        
        var instance =((Instance)context.Scheduler.Context.Get("Instance" + instanceId)!); 
        // instance.CalendarSync.Refresh();
        instance.ConfigurationManager.Load();
        
        return Task.CompletedTask;
    }
}