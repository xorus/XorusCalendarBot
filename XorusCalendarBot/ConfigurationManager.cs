using XorusCalendarBot.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace XorusCalendarBot;

public class ConfigurationManager
{
    public AppConfig AppConfig = new();

    private string _configPath = "config.yml";

    public ConfigurationManager()
    {
        _configPath = Environment.GetEnvironmentVariable("CONFIG_FILE") ?? _configPath;
        Console.WriteLine(_configPath);
        Load();
    }

    public async void Save()
    {
        var serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
        var yaml = serializer.Serialize(AppConfig);

        await File.WriteAllTextAsync(_configPath, yaml);
    }

    public void Load()
    {
        if (!File.Exists(_configPath))
        {
            Save();
        }

        var deser = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        // try
        // {
        AppConfig = deser.Deserialize<AppConfig>(File.ReadAllText(_configPath));
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