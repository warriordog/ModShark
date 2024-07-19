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
            .Entity<MSQueuedUser>()
            .HasOne(q => q.User)
            .WithOne(u => u.QueuedUser)
            .HasForeignKey<MSQueuedUser>(q => q.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        // // FK ms_queued_user(user_id) -> ms_queued_user(user_id)  
        // modelBuilder
        //     .Entity<MSQueuedUser>()
        //     .HasOne(q => q.FlaggedUser)
        //     .WithOne(f => f.QueuedUser)
        //     .HasForeignKey<MSQueuedUser>(q => q.UserId)
        //     .HasPrincipalKey<MSFlaggedUser>(f => f.UserId)
        //     .IsRequired()
        //     .OnDelete(DeleteBehavior.Cascade);
        
        // FK ms_flagged_user(user_id) -> ?user(id)  
        modelBuilder
            .Entity<MSFlaggedUser>()
            .HasOne(f => f.User)
            .WithOne(u => u.FlaggedUser)
            .HasForeignKey<MSFlaggedUser>(f => f.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
        
        // FK ms_flagged_user(user_id) -> ?ms_queued_user(user_id)  
        modelBuilder
            .Entity<MSFlaggedUser>()
            .HasOne(f => f.QueuedUser)
            .WithOne(q => q.FlaggedUser)
            .HasForeignKey<MSFlaggedUser>(f => f.UserId)
            .HasPrincipalKey<MSQueuedUser>(q => q.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
        
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
            .Entity<MSQueuedInstance>()
            .HasOne(q => q.Instance)
            .WithOne(i => i.QueuedInstance)
            .HasForeignKey<MSQueuedInstance>(q => q.InstanceId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        // FK ms_flagged_instance(instance_id) -> ?instance(id)  
        modelBuilder
            .Entity<MSFlaggedInstance>()
            .HasOne(f => f.Instance)
            .WithOne(i => i.FlaggedInstance)
            .HasForeignKey<MSFlaggedInstance>(f => f.InstanceId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
        
        // FK ms_flagged_instance(instance_id) -> ?ms_queued_instance(instance_id)  
        modelBuilder
            .Entity<MSFlaggedInstance>()
            .HasOne(f => f.QueuedInstance)
            .WithOne(q => q.FlaggedInstance)
            .HasForeignKey<MSFlaggedInstance>(f => f.InstanceId)
            .HasPrincipalKey<MSQueuedInstance>(q => q.InstanceId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
        
        // FK ms_queued_note(note_id) -> note(id)  
        modelBuilder
            .Entity<MSQueuedNote>()
            .HasOne(q => q.Note)
            .WithOne(n => n.QueuedNote)
            .HasForeignKey<MSQueuedNote>(q => q.NoteId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        // FK ms_flagged_note(note_id) -> ?note(id)  
        modelBuilder
            .Entity<MSFlaggedNote>()
            .HasOne(q => q.Note)
            .WithOne(n => n.FlaggedNote)
            .HasForeignKey<MSFlaggedNote>(q => q.NoteId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
        
        // FK ms_flagged_note(note_id) -> ?ms_queued_note(note_id)  
        modelBuilder
            .Entity<MSFlaggedNote>()
            .HasOne(f => f.QueuedNote)
            .WithOne(q => q.FlaggedNote)
            .HasForeignKey<MSFlaggedNote>(f => f.NoteId)
            .HasPrincipalKey<MSQueuedNote>(q => q.NoteId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);
        
        // FK note(userId)* -> user(id)  
        modelBuilder
            .Entity<Note>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notes)
            .HasForeignKey(n => n.UserId)
            .IsRequired();
        
        // FK note(userHost)* -> ?instance(host)
        modelBuilder
            .Entity<Note>()
            .HasOne(n => n.Instance)
            .WithMany(i => i.Notes)
            .HasForeignKey(n => n.UserHost)
            .HasPrincipalKey(i => i.Host)
            .IsRequired(false);
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