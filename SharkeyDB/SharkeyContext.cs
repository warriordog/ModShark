using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using SharkeyDB.Entities;

namespace SharkeyDB;

[PublicAPI]
public class SharkeyDBConfig
{
    public required string Connection { get; set; }
    public int Timeout { get; set; } = 30;
}

public class SharkeyContext(DbContextOptions<SharkeyContext> options, SharkeyDBConfig config) : DbContext(options)
{
    public DbSet<Meta> Metas { get; set; }
    
    public DbSet<User> Users { get; set; }
    public DbSet<MSFlaggedUser> MSFlaggedUsers { get; set; }
    public DbSet<MSQueuedUser> MSQueuedUsers { get; set; }
    
    public DbSet<Instance> Instances { get; set; }
    public DbSet<MSFlaggedInstance> MSFlaggedInstances { get; set; }
    public DbSet<MSQueuedInstance> MSQueuedInstances { get; set; }
    
    public DbSet<Note> Notes { get; set; }
    public DbSet<MSFlaggedNote> MSFlaggedNotes { get; set; }
    public DbSet<MSQueuedNote> MSQueuedNotes { get; set; }
    
    public DbSet<AbuseUserReport> AbuseUserReports { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Disable migrations for Sharkey tables
        // https://devblogs.microsoft.com/dotnet/announcing-entity-framework-core-efcore-5-0-rc1/#exclude-tables-from-migrations
        modelBuilder
            .Entity<User>()
            .ToTable(t => t.ExcludeFromMigrations());
        modelBuilder
            .Entity<Instance>()
            .ToTable(t => t.ExcludeFromMigrations());
        modelBuilder
            .Entity<AbuseUserReport>()
            .ToTable(t => t.ExcludeFromMigrations());
        modelBuilder
            .Entity<Meta>()
            .ToTable(t => t.ExcludeFromMigrations());
        modelBuilder
            .Entity<Note>()
            .ToTable(t => t.ExcludeFromMigrations());

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
            .HasPrincipalKey(i => i.Host)
            .IsRequired(false);
        
        // FK ms_queued_instance(instance_id) -> instance(id)  
        modelBuilder
            .Entity<Instance>()
            .HasOne(i => i.MSQueuedInstance)
            .WithOne(q => q.Instance)
            .HasForeignKey<MSQueuedInstance>(q => q.InstanceId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        // FK ms_queued_note(note_id) -> note(id)  
        modelBuilder
            .Entity<Note>()
            .HasOne(n => n.MSQueuedNote)
            .WithOne(q => q.Note)
            .HasForeignKey<MSQueuedNote>(q => q.NoteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        // FK note(userId)* -> user(id)  
        modelBuilder
            .Entity<Note>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notes)
            .HasForeignKey(n => n.UserId)
            .IsRequired();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // We have to set a long timeout for migrations, due to the data backfill operation.
        // https://stackoverflow.com/a/78015946
        var timeout = EF.IsDesignTime
            ? 600 // 10 minutes
            : config.Timeout;
        
        // https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-strings#aspnet-core
        options.UseNpgsql(config.Connection, o => o.CommandTimeout(timeout));
    }
}