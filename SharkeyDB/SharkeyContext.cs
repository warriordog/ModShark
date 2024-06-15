using Microsoft.EntityFrameworkCore;
using SharkeyDB.Entities;

namespace SharkeyDB;

public class SharkeyContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<MSFlaggedUser> MSFlaggedUsers { get; set; }
    public DbSet<MSQueuedUser> MSQueuedUsers { get; set; }
    
    public DbSet<Instance> Instances { get; set; }
    public DbSet<MSFlaggedInstance> MSFlaggedInstances { get; set; }
    public DbSet<MSQueuedInstance> MSQueuedInstances { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Disable migrations for Sharkey tables
        // https://devblogs.microsoft.com/dotnet/announcing-entity-framework-core-efcore-5-0-rc1/#exclude-tables-from-migrations
        modelBuilder
            .Entity<User>()
            .ToTable("user", t => t.ExcludeFromMigrations());
        modelBuilder
            .Entity<Instance>()
            .ToTable("instance", t => t.ExcludeFromMigrations());

        modelBuilder
            .Entity<User>()
            .HasOne(u => u.MSQueuedUser)
            .WithOne(q => q.User)
            .HasForeignKey<MSQueuedUser>(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder
            .Entity<Instance>()
            .HasOne(i => i.MSQueuedInstance)
            .WithOne(q => q.Instance)
            .HasForeignKey<MSQueuedInstance>(q => q.InstanceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}