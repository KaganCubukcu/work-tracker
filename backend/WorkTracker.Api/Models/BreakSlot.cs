namespace WorkTracker.Api.Models;

public class BreakSlot
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;   // örn. "Sabah Molası", "Öğle Yemeği"
    public TimeOnly StartTime { get; set; }              // örn. 10:00
    public TimeOnly EndTime { get; set; }                 // örn. 10:10
    public DateTime? DeletedAt { get; set; }
}