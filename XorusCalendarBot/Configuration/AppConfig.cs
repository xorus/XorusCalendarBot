namespace XorusCalendarBot.Configuration;

public class AppConfig
{
    public string BotToken { get; set; } = "somediscordbottoken";

    public List<InstanceConfig> Instances = new();

    public AppConfig()
    {
        Instances.Add(new InstanceConfig());
    }
}