using Microsoft.EntityFrameworkCore;
using SharkeyDB.Entities;

namespace SharkeyDB;


public class SharkeyContext(DbContextOptions<SharkeyContext> options, SharkeyDBConfig config) : DbContext(options)
{
    public DbSet<Meta> Metas { get; set; }
    
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
            .Entity<AbuseUserReport>()
            .ToTable("abuse_user_report", t => t.ExcludeFromMigrations());
        modelBuilder
            .Entity<Meta>()
            .ToTable("meta", t => t.ExcludeFromMigrations());

        // FK ms_queued_user(user_id) -> user(id)  
        modelBuilder
            .Entity<User>()
            .HasOne(u => u.MSQueuedUser)
            .WithOne(q => q.User)
            .HasForeignKey<MSQueuedUser>(q => q.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        // FK abuse_user_report(reporter_id)* -> user(id)  
        modelBuilder
            .Entity<User>()
            .HasMany(u => u.ReportsBy)
            .WithOne(r => r.Reporter)
            .HasForeignKey(r => r.ReporterId)
            .IsRequired();
        
        // FK abuse_user_report(target_user_id)* -> user(id)  
        modelBuilder
            .Entity<User>()
            .HasMany(u => u.ReportsAgainst)
            .WithOne(r => r.TargetUser)
            .HasForeignKey(r => r.TargetUserId)
            .IsRequired();
        
        // FK abuse_user_report(assignee_id)* -> ?user(id)  
        modelBuilder
            .Entity<User>()
            .HasMany(u => u.ReportsAssignedTo)
            .WithOne(r => r.Assignee)
            .HasForeignKey(r => r.AssigneeId)
            .IsRequired(false);
        
        // FK user(host)* -> ?instance(host)
        modelBuilder
            .Entity<User>()
            .HasOne(u => u.Instance)
            .WithMany(i => i.Users)
            .HasForeignKey(u => u.Host)
            .IsRequired(false);
        
        // FK ms_queued_instance(instance_id) -> instance(id)  
        modelBuilder
            .Entity<Instance>()
            .HasOne(i => i.MSQueuedInstance)
            .WithOne(q => q.Instance)
            .HasForeignKey<MSQueuedInstance>(q => q.InstanceId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        // https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-strings#aspnet-core
        => options.UseNpgsql(config.Connection);
}