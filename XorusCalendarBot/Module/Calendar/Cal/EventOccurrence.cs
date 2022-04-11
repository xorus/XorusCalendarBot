namespace XorusCalendarBot.Module.Calendar.Cal;

public class EventOccurrence
{
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public DateTime NotifyTime { get; init; }
    public string? Summary { get; init; }
    public string? Description { get; init; }
    
    // might desync with the actual sent message (only for web client preview purposes)
    public string? Message { get; set; }
    public bool IsForced { get; init; } = false;
}