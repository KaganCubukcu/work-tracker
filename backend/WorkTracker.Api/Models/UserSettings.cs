namespace WorkTracker.Api.Models;

public class UserSettings
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly? HireDate { get; set; }
}
