namespace XorusCalendarBot.Configuration;

public class AppConfig
{
    public List<InstanceConfig> Instances = new();

    public AppConfig()
    {
        Instances.Add(new InstanceConfig());
    }

    public string BotToken { get; set; } = "somediscordbottoken";
}