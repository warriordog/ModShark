using Microsoft.EntityFrameworkCore;
using SharkeyDB.Entities;

namespace SharkeyDB;


public class SharkeyContext(DbContextOptions<SharkeyContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<MSFlaggedUser> MSFlaggedUsers { get; set; }
    public DbSet<MSQueuedUser> MSQueuedUsers { get; set; }
    
    public DbSet<Instance> Instances { get; set; }
    public DbSet<MSFlaggedInstance> MSFlaggedInstances { get; set; }
    public DbSet<MSQueuedInstance> MSQueuedInstances { get; set; }
    
    public DbSet<AbuseUserReport> AbuseUserReports { get; set; }
    
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
            .Entity<User>()
            .HasMany(u => u.ReportsBy)
            .WithOne(r => r.Reporter)
            .HasForeignKey(r => r.ReporterId)
            .IsRequired();
        modelBuilder
            .Entity<User>()
            .HasMany(u => u.ReportsAgainst)
            .WithOne(r => r.TargetUser)
            .HasForeignKey(r => r.TargetUserId)
            .IsRequired();
        modelBuilder
            .Entity<User>()
            .HasMany(u => u.ReportsAssignedTo)
            .WithOne(r => r.Assignee)
            .HasForeignKey(r => r.AssigneeId);
        
        modelBuilder
            .Entity<Instance>()
            .HasOne(i => i.MSQueuedInstance)
            .WithOne(q => q.Instance)
            .HasForeignKey<MSQueuedInstance>(q => q.InstanceId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}