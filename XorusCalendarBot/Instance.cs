using XorusCalendarBot.Calendar;
using XorusCalendarBot.Configuration;
using XorusCalendarBot.Discord;

namespace XorusCalendarBot;

public class Instance
{
    public readonly CalendarSync CalendarSync;
    public readonly Database.CalendarEntity CalendarEntity;
    public readonly ConfigurationManager ConfigurationManager;
    public readonly DiscordManager DiscordManager;

    public Instance(ConfigurationManager configurationManager, Database.CalendarEntity calendarEntity, CalendarSync calendarSync,
        DiscordManager discordManager)
    {
        ConfigurationManager = configurationManager;
        CalendarEntity = calendarEntity;
        CalendarSync = calendarSync;
        DiscordManager = discordManager;
    }
}