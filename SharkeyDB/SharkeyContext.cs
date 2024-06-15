using Microsoft.EntityFrameworkCore;
using SharkeyDB.Entities;

namespace SharkeyDB;

public class SharkeyContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<MSFlaggedUser> MSFlaggedUsers { get; set; }
    public DbSet<MSQueuedUser> MSQueuedUsers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Disable migrations for Sharkey tables
        // https://devblogs.microsoft.com/dotnet/announcing-entity-framework-core-efcore-5-0-rc1/#exclude-tables-from-migrations
        modelBuilder
            .Entity<User>()
            .ToTable("user", t => t.ExcludeFromMigrations());

        modelBuilder
            .Entity<User>()
            .HasOne(u => u.MSQueuedUser)
            .WithOne(q => q.User)
            .HasForeignKey<MSQueuedUser>(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}