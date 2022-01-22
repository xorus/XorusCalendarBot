namespace XorusCalendarBot.Configuration;

public class AppConfig
{
    public List<Database.CalendarEntity> Instances = new();

    public AppConfig()
    {
        Instances.Add(new Database.CalendarEntity());
    }
}