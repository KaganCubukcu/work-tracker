namespace WorkTracker.Api.Models;

public class BreakSlot : IUserOwned, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Label { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateTime? DeletedAt { get; set; }
}
