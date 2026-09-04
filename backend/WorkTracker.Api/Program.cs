using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WorkTracker.Api.Auth;
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

builder.Services.AddSingleton<TokenService>();

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("Jwt:SigningKey is not configured. Set it via 'dotnet user-secrets set Jwt:SigningKey <value>'.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();

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

app.UseAuthentication();
app.UseAuthorization();

// ---- Auth Endpoints ----
var auth = app.MapGroup("/api/auth");

auth.MapPost("/signup", async (AppDbContext db, TokenService tokens, SignupRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Email, username ve password zorunludur.");

    if (req.Password.Length < 8)
        return Results.BadRequest("Şifre en az 8 karakter olmalıdır.");

    var email = req.Email.Trim().ToLowerInvariant();

    var emailTaken = await db.Users.AnyAsync(u => u.Email == email && u.DeletedAt == null);
    if (emailTaken) return Results.Conflict("Bu email zaten kayıtlı.");

    var usernameTaken = await db.Users.AnyAsync(u => u.Username == req.Username && u.DeletedAt == null);
    if (usernameTaken) return Results.Conflict("Bu kullanıcı adı zaten kayıtlı.");

    var user = new User
    {
        Id = Guid.NewGuid(),
        Email = email,
        Username = req.Username,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
        CreatedAt = DateTime.UtcNow,
    };
    db.Users.Add(user);

    var (rawRefreshToken, refreshTokenEntity) = CreateRefreshToken(user.Id, tokens);
    db.RefreshTokens.Add(refreshTokenEntity);

    await db.SaveChangesAsync();

    var accessToken = tokens.CreateAccessToken(user);
    return Results.Created($"/api/auth/me", new AuthResponse(user.Id, user.Email, user.Username, accessToken, rawRefreshToken));
});

auth.MapPost("/login", async (AppDbContext db, TokenService tokens, LoginRequest req) =>
{
    var email = req.Email.Trim().ToLowerInvariant();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null);

    if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
        return Results.Unauthorized();

    user.LastLoginAt = DateTime.UtcNow;

    var (rawRefreshToken, refreshTokenEntity) = CreateRefreshToken(user.Id, tokens);
    db.RefreshTokens.Add(refreshTokenEntity);

    await db.SaveChangesAsync();

    var accessToken = tokens.CreateAccessToken(user);
    return Results.Ok(new AuthResponse(user.Id, user.Email, user.Username, accessToken, rawRefreshToken));
});

auth.MapPost("/refresh", async (AppDbContext db, TokenService tokens, RefreshRequest req) =>
{
    var hash = TokenService.HashRefreshToken(req.RefreshToken);
    var existing = await db.RefreshTokens
        .Include(rt => rt.User)
        .FirstOrDefaultAsync(rt => rt.TokenHash == hash);

    if (existing is null || existing.RevokedAt != null || existing.ExpiresAt < DateTime.UtcNow || existing.User.DeletedAt != null)
        return Results.Unauthorized();

    existing.RevokedAt = DateTime.UtcNow;

    var (rawRefreshToken, refreshTokenEntity) = CreateRefreshToken(existing.UserId, tokens);
    db.RefreshTokens.Add(refreshTokenEntity);

    await db.SaveChangesAsync();

    var accessToken = tokens.CreateAccessToken(existing.User);
    return Results.Ok(new AuthResponse(existing.User.Id, existing.User.Email, existing.User.Username, accessToken, rawRefreshToken));
});

auth.MapPost("/logout", async (AppDbContext db, LogoutRequest req) =>
{
    var hash = TokenService.HashRefreshToken(req.RefreshToken);
    var existing = await db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash);

    if (existing is not null && existing.RevokedAt is null)
    {
        existing.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    return Results.NoContent();
}).RequireAuthorization();

auth.MapGet("/me", async (AppDbContext db, ClaimsPrincipal principal) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == principal.GetUserId() && u.DeletedAt == null);
    if (user is null) return Results.NotFound();

    return Results.Ok(new MeResponse(user.Id, user.Email, user.Username));
}).RequireAuthorization();

static (string raw, RefreshToken entity) CreateRefreshToken(Guid userId, TokenService tokens)
{
    var raw = TokenService.GenerateRefreshToken();
    var entity = new RefreshToken
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        TokenHash = TokenService.HashRefreshToken(raw),
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddDays(tokens.RefreshTokenDays),
    };
    return (raw, entity);
}

// ---- Todo Endpoints ----
var todos = app.MapGroup("/api/todos").RequireAuthorization();

todos.MapGet("/", async (AppDbContext db, ClaimsPrincipal user) =>
    await db.TodoItems
        .Where(t => t.UserId == user.GetUserId() && t.DeletedAt == null)
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
    var todo = await db.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.GetUserId() && t.DeletedAt == null);
    if (todo is null) return Results.NotFound();

    todo.Title = updated.Title;
    todo.IsDone = updated.IsDone;
    todo.CompletedAt = updated.IsDone ? DateTime.UtcNow : null;

    await db.SaveChangesAsync();
    return Results.Ok(todo);
});

todos.MapDelete("/{id}", async (AppDbContext db, ClaimsPrincipal user, Guid id) =>
{
    var todo = await db.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.GetUserId() && t.DeletedAt == null);
    if (todo is null) return Results.NotFound();

    todo.DeletedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ---- WorkSession Endpoints ----
var sessions = app.MapGroup("/api/work-sessions").RequireAuthorization();

sessions.MapGet("/today", async (AppDbContext db, ClaimsPrincipal user) =>
{
    var today = DateOnly.FromDateTime(DateTime.UtcNow);
    var session = await db.WorkSessions
        .FirstOrDefaultAsync(s => s.Date == today && s.UserId == user.GetUserId() && s.DeletedAt == null);

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
    var session = await db.WorkSessions.FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.GetUserId() && s.DeletedAt == null);
    if (session is null) return Results.NotFound();

    session.StartTime = updated.StartTime;
    session.ExpectedDailyHours = updated.ExpectedDailyHours;
    session.IsManuallyEdited = true;

    await db.SaveChangesAsync();
    return Results.Ok(session);
});

// ---- DailyLog Endpoints ----
var logs = app.MapGroup("/api/daily-logs").RequireAuthorization();

logs.MapGet("/today", async (AppDbContext db, ClaimsPrincipal user) =>
{
    var today = DateOnly.FromDateTime(DateTime.UtcNow);

    var entries = await db.DailyLogs
        .Where(l => l.UserId == user.GetUserId() && l.DeletedAt == null)
        .Where(l => l.CreatedAt.Year == today.Year
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
    var log = await db.DailyLogs.FirstOrDefaultAsync(l => l.Id == id && l.UserId == user.GetUserId() && l.DeletedAt == null);
    if (log is null) return Results.NotFound();

    log.DeletedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

logs.MapPut("/{id}", async (AppDbContext db, ClaimsPrincipal user, Guid id, DailyLog updated) =>
{
    var log = await db.DailyLogs.FirstOrDefaultAsync(l => l.Id == id && l.UserId == user.GetUserId() && l.DeletedAt == null);
    if (log is null) return Results.NotFound();

    log.Content = updated.Content;

    await db.SaveChangesAsync();
    return Results.Ok(log);
});

// ---- BreakSlot Endpoints ----
var breaks = app.MapGroup("/api/break-slots").RequireAuthorization();

breaks.MapGet("/", async (AppDbContext db, ClaimsPrincipal user) =>
    await db.BreakSlots
        .Where(b => b.UserId == user.GetUserId() && b.DeletedAt == null)
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
    var slot = await db.BreakSlots.FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.GetUserId() && b.DeletedAt == null);
    if (slot is null) return Results.NotFound();

    slot.Label = updated.Label;
    slot.StartTime = updated.StartTime;
    slot.EndTime = updated.EndTime;

    await db.SaveChangesAsync();
    return Results.Ok(slot);
});

breaks.MapDelete("/{id}", async (AppDbContext db, ClaimsPrincipal user, Guid id) =>
{
    var slot = await db.BreakSlots.FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.GetUserId() && b.DeletedAt == null);
    if (slot is null) return Results.NotFound();

    slot.DeletedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ---- UserSettings Endpoints ----
var settings = app.MapGroup("/api/settings").RequireAuthorization();

settings.MapGet("/", async (AppDbContext db, ClaimsPrincipal user) =>
{
    var s = await db.UserSettings.FirstOrDefaultAsync(x => x.UserId == user.GetUserId());

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
    var s = await db.UserSettings.FirstOrDefaultAsync(x => x.UserId == user.GetUserId());

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

// ---- History Endpoints ----
var history = app.MapGroup("/api/history").RequireAuthorization();

history.MapGet("/{date}", async (AppDbContext db, ClaimsPrincipal user, string date) =>
{
    if (!DateOnly.TryParse(date, out var targetDate))
    {
        return Results.BadRequest("Geçersiz tarih formatı. YYYY-MM-DD kullanın.");
    }

    var session = await db.WorkSessions
        .FirstOrDefaultAsync(s => s.Date == targetDate && s.UserId == user.GetUserId() && s.DeletedAt == null);

    var todos = await db.TodoItems
        .Where(t => t.UserId == user.GetUserId() && t.DeletedAt == null)
        .Where(t => t.CreatedAt.Year == targetDate.Year
                 && t.CreatedAt.Month == targetDate.Month
                 && t.CreatedAt.Day == targetDate.Day)
        .OrderBy(t => t.CreatedAt)
        .ToListAsync();

    var logs = await db.DailyLogs
        .Where(l => l.UserId == user.GetUserId() && l.DeletedAt == null)
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

history.MapGet("/", async (AppDbContext db, ClaimsPrincipal user) =>
{
    var sessionDates = await db.WorkSessions
        .Where(s => s.UserId == user.GetUserId() && s.DeletedAt == null)
        .Select(s => s.Date)
        .ToListAsync();

    return Results.Ok(sessionDates.OrderByDescending(d => d));
});

// ---- DailyLog Search ----
logs.MapGet("/search", async (AppDbContext db, ClaimsPrincipal user, string? q, DateOnly? from, DateOnly? to) =>
{
    var query = db.DailyLogs.Where(l => l.UserId == user.GetUserId() && l.DeletedAt == null).AsQueryable();

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

app.Run();
