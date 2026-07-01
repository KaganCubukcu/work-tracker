using Microsoft.EntityFrameworkCore;
using WorkTracker.Api.Models;

namespace WorkTracker.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<WorkSession> WorkSessions => Set<WorkSession>();
}