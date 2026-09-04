using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WorkTracker.Api.Auth;
using WorkTracker.Api.Data;
using WorkTracker.Api.Models;

namespace WorkTracker.Api.Endpoints;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var todos = app.MapGroup("/api/todos").RequireAuthorization();

        todos.MapGet("/", async (AppDbContext db, ClaimsPrincipal user) =>
            await db.TodoItems
                .Where(t => t.UserId == user.GetUserId())
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync());

        todos.MapPost("/", async (AppDbContext db, ClaimsPrincipal user, TodoItem todo) =>
        {
            todo.Id = Guid.NewGuid();
            todo.UserId = user.GetUserId();
            todo.CreatedAt = DateTime.UtcNow;
            db.TodoItems.Add(todo);
            await db.SaveChangesAsync();
            return Results.Created($"/api/todos/{todo.Id}", todo);
        });

        todos.MapPut("/{id}", async (AppDbContext db, ClaimsPrincipal user, Guid id, TodoItem updated) =>
        {
            var todo = await db.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.GetUserId());
            if (todo is null) return Results.NotFound();

            todo.Title = updated.Title;
            todo.IsDone = updated.IsDone;
            todo.CompletedAt = updated.IsDone ? DateTime.UtcNow : null;

            await db.SaveChangesAsync();
            return Results.Ok(todo);
        });

        todos.MapDelete("/{id}", async (AppDbContext db, ClaimsPrincipal user, Guid id) =>
        {
            var todo = await db.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.GetUserId());
            if (todo is null) return Results.NotFound();

            todo.DeletedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
