namespace WorkTracker.Api.Models;

public interface IUserOwned
{
    Guid UserId { get; set; }
}

public interface ISoftDeletable
{
    DateTime? DeletedAt { get; set; }
}
