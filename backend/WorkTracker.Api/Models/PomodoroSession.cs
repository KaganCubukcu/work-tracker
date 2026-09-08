namespace WorkTracker.Api.Models;

public enum PomodoroSessionType
{
    Work,
    ShortBreak,
    LongBreak
}

public class PomodoroSession : IUserOwned, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public PomodoroSessionType Type { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
