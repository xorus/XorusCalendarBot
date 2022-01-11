using XorusCalendarBot.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace XorusCalendarBot;

public class ConfigurationManager
{
    public AppConfig AppConfig = new();

    public ConfigurationManager()
    {
        Load();
    }

    public async void Save()
    {
        var serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
        var yaml = serializer.Serialize(AppConfig);
        await File.WriteAllTextAsync("config.yaml", yaml);
    }

    public void Load()
    {
        if (!File.Exists("config.yaml"))
        {
            Save();
        }

        var deser = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        // try
        // {
        AppConfig = deser.Deserialize<AppConfig>(File.ReadAllText("config.yaml"));
        // }
        // catch (Exception e)
        // {
        // Console.WriteLine("cannot load" + e.Message);
        // cannot load, just start over
        // Instances.Add(new InstanceConfig());
        // Save();
        // }
        
        OnLoad?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? OnLoad;
}