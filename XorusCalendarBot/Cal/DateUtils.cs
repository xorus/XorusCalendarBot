namespace XorusCalendarBot.Cal;

public static class DateUtils
{
    public static double ToUnixTimestamp(this DateTime date)
    {
        return date.ToUniversalTime().Subtract(DateTime.UnixEpoch).TotalSeconds;
    }
}