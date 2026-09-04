using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WorkTracker.Api.Auth;
using WorkTracker.Api.Data;
using WorkTracker.Api.Models;

namespace WorkTracker.Api.Endpoints;

public static class BreakSlotEndpoints
{
    public static void MapBreakSlotEndpoints(this IEndpointRouteBuilder app)
    {
        var breaks = app.MapGroup("/api/break-slots").RequireAuthorization();

        breaks.MapGet("/", async (AppDbContext db, ClaimsPrincipal user) =>
            await db.BreakSlots
                .Where(b => b.UserId == user.GetUserId())
                .OrderBy(b => b.StartTime)
                .ToListAsync());

        breaks.MapPost("/", async (AppDbContext db, ClaimsPrincipal user, BreakSlot slot) =>
        {
            slot.Id = Guid.NewGuid();
            slot.UserId = user.GetUserId();
            db.BreakSlots.Add(slot);
            await db.SaveChangesAsync();
            return Results.Created($"/api/break-slots/{slot.Id}", slot);
        });

        breaks.MapPut("/{id}", async (AppDbContext db, ClaimsPrincipal user, Guid id, BreakSlot updated) =>
        {
            var slot = await db.BreakSlots.FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.GetUserId());
            if (slot is null) return Results.NotFound();

            slot.Label = updated.Label;
            slot.StartTime = updated.StartTime;
            slot.EndTime = updated.EndTime;

            await db.SaveChangesAsync();
            return Results.Ok(slot);
        });

        breaks.MapDelete("/{id}", async (AppDbContext db, ClaimsPrincipal user, Guid id) =>
        {
            var slot = await db.BreakSlots.FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.GetUserId());
            if (slot is null) return Results.NotFound();

            slot.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
