namespace WorkTracker.Api.Models;

public class WorkSession
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly Date { get; set; }
    public DateTime StartTime { get; set; }
    public double ExpectedDailyHours { get; set; } = 9;
    public bool IsManuallyEdited { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
