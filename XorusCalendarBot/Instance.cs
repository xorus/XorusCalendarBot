using DSharpPlus;
using XorusCalendarBot.Configuration;

namespace XorusCalendarBot;

public class Instance
{
    public readonly ConfigurationManager ConfigurationManager;
    public readonly InstanceConfig Config;
    public readonly CalendarSync CalendarSync;
    public readonly DiscordManager DiscordManager;

    public Instance(ConfigurationManager configurationManager, InstanceConfig instanceConfig, CalendarSync calendarSync, DiscordManager discordManager)
    {
        ConfigurationManager = configurationManager;
        Config = instanceConfig;
        CalendarSync = calendarSync;
        DiscordManager = discordManager;
    }
}