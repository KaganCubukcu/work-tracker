using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WorkTracker.Api.Auth;
using WorkTracker.Api.Data;
using WorkTracker.Api.Models;

namespace WorkTracker.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
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
    }

    private static (string raw, RefreshToken entity) CreateRefreshToken(Guid userId, TokenService tokens)
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
}
