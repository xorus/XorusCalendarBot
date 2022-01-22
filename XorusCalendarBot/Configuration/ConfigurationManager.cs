using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace XorusCalendarBot.Configuration;

public class ConfigurationManager
{
    private readonly string _configPath = "config.yml";
    public AppConfig AppConfig = new();
    public EnvConfig EnvConfig = new();

    public ConfigurationManager()
    {
        _configPath = Environment.GetEnvironmentVariable("CONFIG_FILE") ?? _configPath;
        Console.WriteLine(_configPath);
        Load();
    }

    public async void Save()
    {
        return;
        var serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
        var yaml = serializer.Serialize(AppConfig);

        await File.WriteAllTextAsync(_configPath, yaml);
    }

    public void Load()
    {
        OnLoad?.Invoke(this, EventArgs.Empty);
        return;
        if (!File.Exists(_configPath)) Save();
        AppConfig = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build()
            .Deserialize<AppConfig>(File.ReadAllText(_configPath));

        OnLoad?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? OnLoad;
}