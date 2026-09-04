using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WorkTracker.Api.Auth;
using WorkTracker.Api.Data;
using WorkTracker.Api.Models;

namespace WorkTracker.Api.Endpoints;

public static class DailyLogEndpoints
{
    public static void MapDailyLogEndpoints(this IEndpointRouteBuilder app)
    {
        var logs = app.MapGroup("/api/daily-logs").RequireAuthorization();

        logs.MapGet("/today", async (AppDbContext db, ClaimsPrincipal user) =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var userId = user.GetUserId();

            var entries = await db.DailyLogs
                .Where(l => l.UserId == userId
                         && l.CreatedAt.Year == today.Year
                         && l.CreatedAt.Month == today.Month
                         && l.CreatedAt.Day == today.Day)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return Results.Ok(entries);
        });

        logs.MapPost("/", async (AppDbContext db, ClaimsPrincipal user, DailyLog log) =>
        {
            log.Id = Guid.NewGuid();
            log.UserId = user.GetUserId();
            log.CreatedAt = DateTime.UtcNow;
            db.DailyLogs.Add(log);
            await db.SaveChangesAsync();
            return Results.Created($"/api/daily-logs/{log.Id}", log);
        });

        logs.MapDelete("/{id}", async (AppDbContext db, ClaimsPrincipal user, Guid id) =>
        {
            var log = await db.DailyLogs.FirstOrDefaultAsync(l => l.Id == id && l.UserId == user.GetUserId());
            if (log is null) return Results.NotFound();

            log.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        logs.MapPut("/{id}", async (AppDbContext db, ClaimsPrincipal user, Guid id, DailyLog updated) =>
        {
            var log = await db.DailyLogs.FirstOrDefaultAsync(l => l.Id == id && l.UserId == user.GetUserId());
            if (log is null) return Results.NotFound();

            log.Content = updated.Content;

            await db.SaveChangesAsync();
            return Results.Ok(log);
        });

        logs.MapGet("/search", async (AppDbContext db, ClaimsPrincipal user, string? q, DateOnly? from, DateOnly? to) =>
        {
            var userId = user.GetUserId();
            var query = db.DailyLogs.Where(l => l.UserId == userId);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var loweredQ = q.ToLower();
                query = query.Where(l => l.Content.ToLower().Contains(loweredQ));
            }

            if (from.HasValue)
            {
                var fromDate = from.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(l => l.CreatedAt >= fromDate);
            }

            if (to.HasValue)
            {
                var toDate = to.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(l => l.CreatedAt <= toDate);
            }

            var results = await query
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return Results.Ok(results);
        });
    }
}
