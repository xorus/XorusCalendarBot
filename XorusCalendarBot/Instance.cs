using XorusCalendarBot.Calendar;
using XorusCalendarBot.Configuration;
using XorusCalendarBot.Discord;

namespace XorusCalendarBot;

public class Instance
{
    public readonly CalendarSync CalendarSync;
    public readonly InstanceConfig Config;
    public readonly ConfigurationManager ConfigurationManager;
    public readonly DiscordManager DiscordManager;

    public Instance(ConfigurationManager configurationManager, InstanceConfig instanceConfig, CalendarSync calendarSync,
        DiscordManager discordManager)
    {
        ConfigurationManager = configurationManager;
        Config = instanceConfig;
        CalendarSync = calendarSync;
        DiscordManager = discordManager;
    }
}