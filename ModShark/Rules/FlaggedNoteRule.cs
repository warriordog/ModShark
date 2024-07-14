﻿using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using ModShark.Reports;
using ModShark.Services;
using ModShark.Utils;
using SharkeyDB;
using SharkeyDB.Entities;

namespace ModShark.Rules;

public interface IFlaggedNoteRule : IRule;

[PublicAPI]
public class FlaggedNoteConfig : QueuedRuleConfig
{
    public bool IncludeLocal { get; set; }
    public bool IncludeRemote { get; set; }
    
    public bool IncludeCW { get; set; }
    
    public bool IncludeUnlistedVis { get; set; }
    public bool IncludeFollowersVis { get; set; }
    public bool IncludePrivateVis { get; set; }
    
    public bool IncludeDeletedUser { get; set; }
    public bool IncludeSuspendedUser { get; set; }
    public bool IncludeSilencedUser { get; set; }
    
    public bool IncludeBlockedInstance { get; set; }
    public bool IncludeSilencedInstance { get; set; }
    
    public List<string> TextPatterns { get; set; } = [];
    public int Timeout { get; set; }
}

public class FlaggedNoteRule(ILogger<FlaggedNoteRule> logger, FlaggedNoteConfig config, SharkeyContext db, IMetaService metaService) : QueuedRule<MSQueuedNote>(logger, config, db, db.MSQueuedNotes), IFlaggedNoteRule
{
    // Merge and pre-compile the pattern for efficiency
    private Regex TextPattern { get; } = PatternUtils.CreateMatcher(config.TextPatterns, config.Timeout);

    protected override Task<bool> CanRun(CancellationToken stoppingToken)
    {
        if (config.TextPatterns.Count < 1)
        {
            logger.LogWarning("Skipping run, no patterns defined");
            return Task.FromResult(false);
        }

        if (!config.IncludeLocal && !config.IncludeRemote)
        {
            logger.LogWarning("Skipping run, all notes are excluded (local & remote)");
            return Task.FromResult(false);
        }

        if (!config.IncludeRemote && config.IncludeBlockedInstance)
        {
            logger.LogWarning($"Configuration error: {nameof(FlaggedNoteConfig.IncludeBlockedInstance)} has no effect when {nameof(FlaggedNoteConfig.IncludeRemote)} is false");
        }

        if (!config.IncludeRemote && config.IncludeSilencedInstance)
        {
            logger.LogWarning($"Configuration error: {nameof(FlaggedNoteConfig.IncludeSilencedInstance)} has no effect when {nameof(FlaggedNoteConfig.IncludeRemote)} is false");
        }

        return Task.FromResult(true);
    }

    protected override async Task RunQueuedRule(Report report, int maxId, CancellationToken stoppingToken)
    {
        // Get the list of blocked / silenced instances from metadata
        var meta = await metaService.GetInstanceMeta(stoppingToken);
        
        // Query for all new notes that match the given flags
        var newNotes = db.MSQueuedNotes
            .AsNoTracking()
            .Include(q => q.FlaggedNote)
            .Include(q => q.Note!) // database constraints ensure that "Note" cannot be null
                .ThenInclude(n => n.User!) // database constraints ensure that "User" cannot be null
                    .ThenInclude(u => u.Instance)
            .Where(q =>
                q.Id <= maxId
                && q.FlaggedNote == null
                && (q.Note!.Text != null || (config.IncludeCW && q.Note!.CW != null))
                && (config.IncludeLocal || q.Note!.Url != null)
                && (config.IncludeRemote || q.Note!.Url == null)
                && (config.IncludeUnlistedVis || q.Note!.Visibility != "home")
                && (config.IncludeFollowersVis || q.Note!.Visibility != "followers")
                && (config.IncludePrivateVis || q.Note!.Visibility != "specified")
                && (config.IncludeSuspendedUser || !q.Note!.User!.IsSuspended)
                && (config.IncludeSilencedUser || !q.Note!.User!.IsSilenced)
                && (config.IncludeDeletedUser || !q.Note!.User!.IsDeleted)
                && (config.IncludeBlockedInstance || q.Note!.User!.Host == null || !meta.BlockedHosts.Contains(q.Note!.User!.Host))
                && (config.IncludeSilencedInstance || q.Note!.User!.Host == null || !meta.SilencedHosts.Contains(q.Note!.User!.Host)))
            .OrderBy(q => q.Id)
            .Select(q => q.Note!)
            .AsAsyncEnumerable();
        
        // Stream each result due to potentially large size
        await foreach (var note in newNotes)
        {
            // Guaranteed to be non-null by the Include() statement
            var user = note.User!;
            
            // Check for base domain and alternate-case matches.
            // This cannot be done efficiently in-database.
            if (user.Host != null)
            {
                // The query only excludes exact matches, so check for base domains here
                if (!config.IncludeBlockedInstance && HostUtils.Matches(user.Host, meta.BlockedHosts))
                    continue;
                if (!config.IncludeSilencedInstance && HostUtils.Matches(user.Host, meta.SilencedHosts))
                    continue;
            }

            // For better use of database resources, we handle pattern matching in application code.
            // This also gives us .NET's faster and more powerful regex engine.
            if (!HasMatchedText(note))
                continue;
            
            report.NoteReports.Add(new NoteReport
            {
                Instance = user.Instance,
                User = user,
                Note = note
            });

            db.MSFlaggedNotes.Add(new MSFlaggedNote
            {
                NoteId = note.Id,
                FlaggedAt = report.ReportDate
            });
        }
    }

    private bool HasMatchedText(Note note)
    {
        // Check "text" field
        if (note.Text != null && TextPattern.IsMatch(note.Text))
            return true;

        // Check "cw" field if enabled
        if (config.IncludeCW && note.CW != null && TextPattern.IsMatch(note.CW))
            return true;

        return false;
    }
}