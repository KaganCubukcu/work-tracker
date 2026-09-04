using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WorkTracker.Api.Models;

namespace WorkTracker.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<WorkSession> WorkSessions => Set<WorkSession>();
    public DbSet<DailyLog> DailyLogs => Set<DailyLog>();
    public DbSet<BreakSlot> BreakSlots => Set<BreakSlot>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<UserSettings>().HasIndex(s => s.UserId).IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId);

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
}