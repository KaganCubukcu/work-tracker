namespace WorkTracker.Api.Models;

public class BreakSlot
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateTime? DeletedAt { get; set; }
}