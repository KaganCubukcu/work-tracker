namespace WorkTracker.Api.Models;

public class TodoItem : IUserOwned, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
