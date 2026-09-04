using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WorkTracker.Api.Auth;
using WorkTracker.Api.Data;
using WorkTracker.Api.Models;

namespace WorkTracker.Api.Endpoints;

public static class UserSettingsEndpoints
{
    public static void MapUserSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var settings = app.MapGroup("/api/settings").RequireAuthorization();

        settings.MapGet("/", async (AppDbContext db, ClaimsPrincipal user) =>
        {
            var s = await db.UserSettings.FirstOrDefaultAsync(s => s.UserId == user.GetUserId());

            if (s is null)
            {
                s = new UserSettings { Id = Guid.NewGuid(), UserId = user.GetUserId(), HireDate = null };
                db.UserSettings.Add(s);
                await db.SaveChangesAsync();
            }

            return Results.Ok(s);
        });

        settings.MapPut("/", async (AppDbContext db, ClaimsPrincipal user, UserSettings updated) =>
        {
            var s = await db.UserSettings.FirstOrDefaultAsync(s => s.UserId == user.GetUserId());

            if (s is null)
            {
                s = new UserSettings { Id = Guid.NewGuid(), UserId = user.GetUserId(), HireDate = updated.HireDate };
                db.UserSettings.Add(s);
            }
            else
            {
                s.HireDate = updated.HireDate;
            }

            await db.SaveChangesAsync();
            return Results.Ok(s);
        });
    }
}
