namespace WorkTracker.Api.Models;

public class UserSettings : IUserOwned
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly? HireDate { get; set; }
    public int PomodoroWorkMinutes { get; set; } = 25;
    public int PomodoroShortBreakMinutes { get; set; } = 5;
    public int PomodoroLongBreakMinutes { get; set; } = 15;
    public int PomodoroRoundsBeforeLongBreak { get; set; } = 4;
}
