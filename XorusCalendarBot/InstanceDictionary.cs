using Swan.DependencyInjection;
using XorusCalendarBot.Database;

namespace XorusCalendarBot;

public class InstanceDictionary : IDisposable
{
    public Dictionary<Guid, Instance> Instances { get; set; } = new();

    private readonly DependencyContainer _container;

    private DatabaseManager DatabaseManager => _container.Resolve<DatabaseManager>();

    public InstanceDictionary(DependencyContainer container)
    {
        _container = container;
    }

    public void Init()
    {
        Update();
        DatabaseManager.CalendarEntitiesUpdated += (_, _) => Update();
    }

    void CreateInstances(IEnumerable<CalendarEntity> calendarEntities)
    {
        foreach (var calendarEntity in calendarEntities)
        {
            if (Instances.ContainsKey(calendarEntity.Id))
            {
                var id = calendarEntity.Id;
                Instances[id].Replace(calendarEntity);
            }
            else
            {
                var instance = new Instance(_container, calendarEntity);
                Instances.Add(instance.CalendarEntity.Id, instance);
            }
        }
    }

    public async Task RefreshAsync(Guid calendarId)
    {
        if (Instances.ContainsKey(calendarId))
        {
            await Instances[calendarId].RefreshAsync();
        }
    }

    public void Refresh(CalendarEntity calendarEntity)
    {
        if (Instances.ContainsKey(calendarEntity.Id))
        {
            Instances[calendarEntity.Id].Replace(calendarEntity);
        }
    }

    public void Update()
    {
        CreateInstances(_container.Resolve<DatabaseManager>().CalendarEntityCollection.FindAll());
    }

    public void Dispose()
    {
        foreach (var instance in Instances) instance.Value.Dispose();
        GC.SuppressFinalize(this);
    }
}