using Swan.DependencyInjection;
using XorusCalendarBot.Database;

namespace XorusCalendarBot.Security;

public class UserVoter
{
    public static bool CanEdit(UserEntity user, CalendarEntity calendarEntity)
    {
        return user.IsAdmin || user.EditGuilds.Contains(calendarEntity.GuildId);
    }
}