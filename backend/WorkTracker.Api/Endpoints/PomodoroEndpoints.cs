using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WorkTracker.Api.Auth;
using WorkTracker.Api.Data;
using WorkTracker.Api.Models;

namespace WorkTracker.Api.Endpoints;

public static class PomodoroEndpoints
{
    public static void MapPomodoroEndpoints(this IEndpointRouteBuilder app)
    {
        var pomodoro = app.MapGroup("/api/pomodoro").RequireAuthorization();

        pomodoro.MapGet("/active", async (AppDbContext db, ClaimsPrincipal user) =>
        {
            var active = await db.PomodoroSessions
                .Where(p => p.UserId == user.GetUserId() && p.CompletedAt == null)
                .OrderByDescending(p => p.StartedAt)
                .FirstOrDefaultAsync();

            return Results.Ok(active);
        });

        pomodoro.MapGet("/today", async (AppDbContext db, ClaimsPrincipal user) =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var completed = await db.PomodoroSessions
                .Where(p => p.UserId == user.GetUserId()
                    && p.CompletedAt != null
                    && DateOnly.FromDateTime(p.StartedAt) == today)
                .OrderBy(p => p.StartedAt)
                .ToListAsync();

            return Results.Ok(completed);
        });

        pomodoro.MapPost("/", async (AppDbContext db, ClaimsPrincipal user, PomodoroSession session) =>
        {
            var existingActive = await db.PomodoroSessions
                .Where(p => p.UserId == user.GetUserId() && p.CompletedAt == null)
                .OrderByDescending(p => p.StartedAt)
                .FirstOrDefaultAsync();

            if (existingActive is not null) return Results.Ok(existingActive);

            var pomodoroSession = new PomodoroSession
            {
                Id = Guid.NewGuid(),
                UserId = user.GetUserId(),
                Type = session.Type,
                StartedAt = DateTime.UtcNow
            };
            db.PomodoroSessions.Add(pomodoroSession);
            await db.SaveChangesAsync();
            return Results.Created($"/api/pomodoro/{pomodoroSession.Id}", pomodoroSession);
        });

        pomodoro.MapPut("/{id}/complete", async (AppDbContext db, ClaimsPrincipal user, Guid id) =>
        {
            var session = await db.PomodoroSessions.FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.GetUserId());
            if (session is null) return Results.NotFound();

            session.CompletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(session);
        });

        pomodoro.MapDelete("/{id}", async (AppDbContext db, ClaimsPrincipal user, Guid id) =>
        {
            var session = await db.PomodoroSessions.FirstOrDefaultAsync(p => p.Id == id && p.UserId == user.GetUserId());
            if (session is null) return Results.NotFound();

            session.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
