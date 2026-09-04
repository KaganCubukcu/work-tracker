using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WorkTracker.Api.Auth;
using WorkTracker.Api.Data;
using WorkTracker.Api.Models;

namespace WorkTracker.Api.Endpoints;

public static class WorkSessionEndpoints
{
    public static void MapWorkSessionEndpoints(this IEndpointRouteBuilder app)
    {
        var sessions = app.MapGroup("/api/work-sessions").RequireAuthorization();

        sessions.MapGet("/today", async (AppDbContext db, ClaimsPrincipal user) =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var session = await db.WorkSessions
                .FirstOrDefaultAsync(s => s.Date == today && s.UserId == user.GetUserId());

            if (session is null)
            {
                session = new WorkSession
                {
                    Id = Guid.NewGuid(),
                    UserId = user.GetUserId(),
                    Date = today,
                    StartTime = DateTime.UtcNow,
                    ExpectedDailyHours = 9
                };
                db.WorkSessions.Add(session);
                await db.SaveChangesAsync();
            }

            return Results.Ok(session);
        });

        sessions.MapPut("/{id}", async (AppDbContext db, ClaimsPrincipal user, Guid id, WorkSession updated) =>
        {
            var session = await db.WorkSessions.FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.GetUserId());
            if (session is null) return Results.NotFound();

            session.StartTime = updated.StartTime;
            session.ExpectedDailyHours = updated.ExpectedDailyHours;
            session.IsManuallyEdited = true;

            await db.SaveChangesAsync();
            return Results.Ok(session);
        });
    }
}
