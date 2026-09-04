using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WorkTracker.Api.Auth;
using WorkTracker.Api.Models;

namespace WorkTracker.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : DbContext(options)
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<WorkSession> WorkSessions => Set<WorkSession>();
    public DbSet<DailyLog> DailyLogs => Set<DailyLog>();
    public DbSet<BreakSlot> BreakSlots => Set<BreakSlot>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    private Guid? CurrentUserId
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true) return null;
            return principal.GetUserId();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<UserSettings>().HasIndex(s => s.UserId).IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IUserOwned).IsAssignableFrom(entityType.ClrType)) continue;

            var isSoftDeletable = typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType);
            var filter = BuildOwnershipFilter(entityType.ClrType, isSoftDeletable);
            entityType.SetQueryFilter(filter);
        }

        var utcConverter = new ValueConverter<DateTime, DateTime>(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableUtcConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(utcConverter);
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(nullableUtcConverter);
                }
            }
        }
    }

    private LambdaExpression BuildOwnershipFilter(Type entityType, bool isSoftDeletable)
    {
        var parameter = Expression.Parameter(entityType, "x");
        var currentUserId = Expression.Property(Expression.Constant(this), nameof(CurrentUserId));

        var userIdProperty = Expression.Property(parameter, nameof(IUserOwned.UserId));
        Expression body = Expression.Equal(
            Expression.Convert(userIdProperty, typeof(Guid?)),
            currentUserId);

        if (isSoftDeletable)
        {
            var deletedAtProperty = Expression.Property(parameter, nameof(ISoftDeletable.DeletedAt));
            var notDeleted = Expression.Equal(deletedAtProperty, Expression.Constant(null, typeof(DateTime?)));
            body = Expression.AndAlso(body, notDeleted);
        }

        return Expression.Lambda(body, parameter);
    }
}