namespace WorkTracker.Api.Models;

public class UserSettings : IUserOwned
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly? HireDate { get; set; }
}
