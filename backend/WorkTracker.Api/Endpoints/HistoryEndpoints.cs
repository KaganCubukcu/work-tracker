using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WorkTracker.Api.Auth;
using WorkTracker.Api.Data;

namespace WorkTracker.Api.Endpoints;

public static class HistoryEndpoints
{
    public static void MapHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        var history = app.MapGroup("/api/history").RequireAuthorization();

        history.MapGet("/{date}", async (AppDbContext db, ClaimsPrincipal user, string date) =>
        {
            if (!DateOnly.TryParse(date, out var targetDate))
            {
                return Results.BadRequest("Geçersiz tarih formatı. YYYY-MM-DD kullanın.");
            }

            var userId = user.GetUserId();

            var session = await db.WorkSessions
                .FirstOrDefaultAsync(s => s.Date == targetDate && s.UserId == userId);

            var todos = await db.TodoItems
                .Where(t => t.UserId == userId
                         && t.CreatedAt.Year == targetDate.Year
                         && t.CreatedAt.Month == targetDate.Month
                         && t.CreatedAt.Day == targetDate.Day)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();

            var logs = await db.DailyLogs
                .Where(l => l.UserId == userId
                         && l.CreatedAt.Year == targetDate.Year
                         && l.CreatedAt.Month == targetDate.Month
                         && l.CreatedAt.Day == targetDate.Day)
                .OrderBy(l => l.CreatedAt)
                .ToListAsync();

            return Results.Ok(new
            {
                date = targetDate,
                session,
                todos,
                logs
            });
        });

        history.MapGet("/", async (AppDbContext db, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var sessionDates = await db.WorkSessions
                .Where(s => s.UserId == userId)
                .Select(s => s.Date)
                .ToListAsync();

            return Results.Ok(sessionDates.OrderByDescending(d => d));
        });
    }
}
