using Swan.DependencyInjection;

namespace XorusCalendarBot.Module.Calendar;

public class InstanceDictionary : IDisposable
{
    private Dictionary<Guid, Instance> Instances { get; set; } = new();

    private readonly DependencyContainer _container;

    private CalendarModule CalendarModule => _container.Resolve<CalendarModule>();

    public InstanceDictionary(DependencyContainer container)
    {
        _container = container;
    }

    public void Init()
    {
        Update();
        CalendarModule.CalendarEntitiesUpdated += (_, _) => Update();
    }

    private void CreateInstances(IEnumerable<CalendarEntity> calendarEntities)
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

    public async Task RefreshAsync(CalendarEntity calendarEntity)
    {
        if (Instances.ContainsKey(calendarEntity.Id))
        {
            await Instances[calendarEntity.Id].ReplaceAsync(calendarEntity);
        }
    }

    private void Update()
    {
        CreateInstances(_container.Resolve<CalendarModule>().CalendarEntityCollection.FindAll());
    }

    public void Dispose()
    {
        foreach (var instance in Instances) instance.Value.Dispose();
        GC.SuppressFinalize(this);
    }
}