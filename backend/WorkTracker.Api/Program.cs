using Microsoft.EntityFrameworkCore;
using WorkTracker.Api.Data;
using WorkTracker.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

// ---- Todo Endpoints ----
var todos = app.MapGroup("/api/todos");

todos.MapGet("/", async (AppDbContext db) =>
    await db.TodoItems
        .Where(t => t.DeletedAt == null)
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync());

todos.MapPost("/", async (AppDbContext db, TodoItem todo) =>
{
    todo.CreatedAt = DateTime.UtcNow;
    db.TodoItems.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/api/todos/{todo.Id}", todo);
});

todos.MapPut("/{id}", async (AppDbContext db, int id, TodoItem updated) =>
{
    var todo = await db.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);
    if (todo is null) return Results.NotFound();

    todo.Title = updated.Title;
    todo.IsDone = updated.IsDone;
    todo.CompletedAt = updated.IsDone ? DateTime.UtcNow : null;

    await db.SaveChangesAsync();
    return Results.Ok(todo);
});

todos.MapDelete("/{id}", async (AppDbContext db, int id) =>
{
    var todo = await db.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);
    if (todo is null) return Results.NotFound();

    todo.DeletedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ---- WorkSession Endpoints ----
var sessions = app.MapGroup("/api/work-sessions");

sessions.MapGet("/today", async (AppDbContext db) =>
{
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var session = await db.WorkSessions
        .FirstOrDefaultAsync(s => s.Date == today && s.DeletedAt == null);

    if (session is null)
    {
        session = new WorkSession
        {
            Date = today,
            StartTime = DateTime.UtcNow,
            ExpectedDailyHours = 9
        };
        db.WorkSessions.Add(session);
        await db.SaveChangesAsync();
    }

    return Results.Ok(session);
});

sessions.MapPut("/{id}", async (AppDbContext db, int id, WorkSession updated) =>
{
    var session = await db.WorkSessions.FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null);
    if (session is null) return Results.NotFound();

    session.StartTime = updated.StartTime;
    session.ExpectedDailyHours = updated.ExpectedDailyHours;
    session.IsManuallyEdited = true;

    await db.SaveChangesAsync();
    return Results.Ok(session);
});

// ---- DailyLog Endpoints ----
var logs = app.MapGroup("/api/daily-logs");

// Bugünün notlarını getir (en yeni üstte)
logs.MapGet("/today", async (AppDbContext db) =>
{
    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    var entries = await db.DailyLogs
        .Where(l => l.DeletedAt == null)
        .Where(l => l.CreatedAt.Year == today.Year
                 && l.CreatedAt.Month == today.Month
                 && l.CreatedAt.Day == today.Day)
        .OrderByDescending(l => l.CreatedAt)
        .ToListAsync();

    return Results.Ok(entries);
});

logs.MapPost("/", async (AppDbContext db, DailyLog log) =>
{
    log.CreatedAt = DateTime.UtcNow;
    db.DailyLogs.Add(log);
    await db.SaveChangesAsync();
    return Results.Created($"/api/daily-logs/{log.Id}", log);
});

logs.MapDelete("/{id}", async (AppDbContext db, int id) =>
{
    var log = await db.DailyLogs.FirstOrDefaultAsync(l => l.Id == id && l.DeletedAt == null);
    if (log is null) return Results.NotFound();

    log.DeletedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();