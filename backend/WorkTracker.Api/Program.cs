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

// ---- BreakSlot Endpoints ----
var breaks = app.MapGroup("/api/break-slots");

breaks.MapGet("/", async (AppDbContext db) =>
    await db.BreakSlots
        .Where(b => b.DeletedAt == null)
        .OrderBy(b => b.StartTime)
        .ToListAsync());

breaks.MapPost("/", async (AppDbContext db, BreakSlot slot) =>
{
    db.BreakSlots.Add(slot);
    await db.SaveChangesAsync();
    return Results.Created($"/api/break-slots/{slot.Id}", slot);
});

breaks.MapPut("/{id}", async (AppDbContext db, int id, BreakSlot updated) =>
{
    var slot = await db.BreakSlots.FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);
    if (slot is null) return Results.NotFound();

    slot.Label = updated.Label;
    slot.StartTime = updated.StartTime;
    slot.EndTime = updated.EndTime;

    await db.SaveChangesAsync();
    return Results.Ok(slot);
});

breaks.MapDelete("/{id}", async (AppDbContext db, int id) =>
{
    var slot = await db.BreakSlots.FirstOrDefaultAsync(b => b.Id == id && b.DeletedAt == null);
    if (slot is null) return Results.NotFound();

    slot.DeletedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ---- UserSettings Endpoints ----
var settings = app.MapGroup("/api/settings");

settings.MapGet("/", async (AppDbContext db) =>
{
    var s = await db.UserSettings.FirstOrDefaultAsync();

    if (s is null)
    {
        s = new UserSettings { HireDate = null };
        db.UserSettings.Add(s);
        await db.SaveChangesAsync();
    }

    return Results.Ok(s);
});

settings.MapPut("/", async (AppDbContext db, UserSettings updated) =>
{
    var s = await db.UserSettings.FirstOrDefaultAsync();

    if (s is null)
    {
        s = new UserSettings { HireDate = updated.HireDate };
        db.UserSettings.Add(s);
    }
    else
    {
        s.HireDate = updated.HireDate;
    }

    await db.SaveChangesAsync();
    return Results.Ok(s);
});

// ---- History Endpoints ----
var history = app.MapGroup("/api/history");

history.MapGet("/{date}", async (AppDbContext db, string date) =>
{
    if (!DateOnly.TryParse(date, out var targetDate))
    {
        return Results.BadRequest("Geçersiz tarih formatı. YYYY-MM-DD kullanın.");
    }

    var session = await db.WorkSessions
        .FirstOrDefaultAsync(s => s.Date == targetDate && s.DeletedAt == null);

    var todos = await db.TodoItems
        .Where(t => t.DeletedAt == null)
        .Where(t => t.CreatedAt.Year == targetDate.Year
                 && t.CreatedAt.Month == targetDate.Month
                 && t.CreatedAt.Day == targetDate.Day)
        .OrderBy(t => t.CreatedAt)
        .ToListAsync();

    var logs = await db.DailyLogs
        .Where(l => l.DeletedAt == null)
        .Where(l => l.CreatedAt.Year == targetDate.Year
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

history.MapGet("/", async (AppDbContext db) =>
{
    var sessionDates = await db.WorkSessions
        .Where(s => s.DeletedAt == null)
        .Select(s => s.Date)
        .ToListAsync();

    return Results.Ok(sessionDates.OrderByDescending(d => d));
});

app.Run();