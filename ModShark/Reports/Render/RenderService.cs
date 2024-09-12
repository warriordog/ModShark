using ModShark.Reports.Document;
using ModShark.Services;
using ModShark.Utils;

namespace ModShark.Reports.Render;

public enum FlagInclusion
{
    /// <summary>
    /// Flagged content is not included.
    /// </summary>
    None,
    
    /// <summary>
    /// Only the exact fragment of matched content is included.
    /// </summary>
    Minimal,
    
    /// <summary>
    /// The entire block of flagged content is included.
    /// </summary>
    Full
}

public interface IRenderService
{
    DocumentBuilder RenderReport(Report report, DocumentFormat format, FlagInclusion includeFlags = FlagInclusion.None);
}

public class RenderService(ILinkService linkService) : IRenderService
{
    public DocumentBuilder RenderReport(Report report, DocumentFormat format, FlagInclusion includeFlags = FlagInclusion.None)
    {
        var document = new DocumentBuilder(format);
        document.AppendTitle("ModShark Report");
        
        RenderInstanceReports(document, report, includeFlags);
        RenderUserReports(document, report, includeFlags);
        RenderNoteReports(document, report, includeFlags);

        return document;
    }

    private void RenderInstanceReports(DocumentBuilder document, Report report, FlagInclusion includeFlags)
    {
        if (!report.HasInstanceReports)
            return;

        var section = document.BeginSection();
        
        var count = report.InstanceReports.Count;
        AppendSectionHeader(section, count, "instance");

        var list = section.BeginList();
        foreach (var instanceReport in report.InstanceReports)
        {
            var group = list.BeginGroup();
            
            AppendInstanceReport(group, instanceReport);
            AppendFlags(group, instanceReport.Flags, includeFlags);
            
            group.End();
        }
        list.End();

        section.End();
    }

    private void AppendInstanceReport<T>(ListBuilder<T> list, InstanceReport instanceReport)
        where T : BuilderBase<T>
    {
        var instanceLink = linkService.GetLinkToInstance(instanceReport.Instance);
        var localInstanceLink = linkService.GetLocalLinkToInstance(instanceReport.Instance);
        
        list
            .BeginListItem()
                // Instance remote link
                .AppendText("Remote instance ")
                .BeginLink(instanceLink)
                    .AppendCode(instanceReport.Instance.Id)
                    .AppendText(" (", instanceReport.Instance.Host, ")")
                .End()

                // instance local link
                .AppendText(" ")
                .BeginLink(localInstanceLink)
                    .AppendItalics("[local mirror]")
                .End()
            .End();
    }

    private void RenderUserReports(DocumentBuilder document, Report report, FlagInclusion includeFlags)
    {
        if (!report.HasUserReports)
            return;
        
        var section = document.BeginSection();
        
        var count = report.UserReports.Count;
        AppendSectionHeader(section, count, "user");

        var list = section.BeginList();
        foreach (var userReport in report.UserReports)
        {
            var group = list.BeginGroup();
            
            if (userReport.IsLocal)
                AppendLocalUserReport(group, userReport);
            else
                AppendRemoteUserReport(group, userReport);

            AppendFlags(group, userReport.Flags, includeFlags);

            group.End();
        }
        list.End();

        section.End();
    }

    private void AppendLocalUserReport<T>(ListBuilder<T> list, UserReport userReport)
        where T : BuilderBase<T>
    {
        if (!userReport.IsLocal)
            return;
        
        var userLink = linkService.GetLinkToUser(userReport.User);

        // User local link
        list
            .BeginListItem()
                .BeginBold()
                    .AppendText("Local user ")
                    .BeginLink(userLink)
                        .AppendCode(userReport.User.Id)
                        .AppendText($" ({userReport.User.Username})")
                    .End()
                .End()
            .End();
    }
    
    private void AppendRemoteUserReport<T>(ListBuilder<T> list, UserReport userReport)
        where T : BuilderBase<T>
    {
        if (userReport.IsLocal)
            return;
        
        var userLink = linkService.GetLinkToUser(userReport.User);
        var instanceLink = linkService.GetLinkToInstance(userReport.Instance);
        var localInstanceLink = linkService.GetLocalLinkToInstance(userReport.Instance);
        var localUserLink = linkService.GetLocalLinkToUser(userReport.User);

        
        // User
        list
            .BeginListItem()
                // User remote link
                .AppendText("Remote user ")
                    .BeginLink(userLink)
                        .AppendCode(userReport.User.Id)
                    .AppendText($" ({userReport.User.Username}@{userReport.Instance.Host})")
                .End()
                        
                // User local link
                .AppendText(" ")
                    .BeginLink(localUserLink)
                    .AppendItalics("[local mirror]")
                .End()
            .End();
            
        // User's instance
        list
            .BeginList()
                .BeginListItem()
                    // Instance remote link
                    .AppendText("from instance ")
                    .BeginLink(instanceLink)
                        .AppendCode(userReport.Instance.Id)
                        .AppendText($" ({userReport.Instance.Host})")
                    .End()

                    // instance local link
                    .AppendText(" ")
                    .BeginLink(localInstanceLink)
                        .AppendItalics("[local mirror]")
                    .End()
                .End()
            .End();
    }

    private void RenderNoteReports(DocumentBuilder document, Report report, FlagInclusion includeFlags)
    {
        if (!report.HasNoteReports)
            return;
        
        var section = document.BeginSection();
        
        var count = report.NoteReports.Count;
        AppendSectionHeader(section, count, "note");

        var list = section.BeginList();
        foreach (var noteReport in report.NoteReports)
        {
            var group = list.BeginGroup();
            
            if (noteReport.IsLocal)
                AppendLocalNoteReport(group, noteReport);
            else
                AppendRemoteNoteReport(group, noteReport);

            AppendFlags(group, noteReport.Flags, includeFlags);

            group.End();
        }
        list.End();

        section.End();
    }

    private void AppendLocalNoteReport<T>(ListBuilder<T> list, NoteReport noteReport)
        where T : BuilderBase<T>
    {
        if (!noteReport.IsLocal)
            return;
        
        var noteLink = linkService.GetLinkToNote(noteReport.Note);
        var userLink = linkService.GetLinkToUser(noteReport.User);
        
        // note local link
        list
            .BeginListItem()
                .BeginBold()
                    .AppendText("Local note ")
                        .BeginLink(noteLink)
                        .AppendCode(noteReport.Note.Id)
                    .End()
                .End()
            .End();
                
        // user local link
        list
            .BeginList()
                .BeginListItem()
                    .AppendText("by user ")
                    .BeginLink(userLink)
                        .AppendCode(noteReport.User.Id)
                        .AppendText($" ({noteReport.User.Username})")
                    .End()
                .End()
            .End();
    }

    private void AppendRemoteNoteReport<T>(ListBuilder<T> list, NoteReport noteReport)
        where T : BuilderBase<T>
    {
        if (noteReport.IsLocal)
            return;
        
        var noteLink = linkService.GetLinkToNote(noteReport.Note);
        var userLink = linkService.GetLinkToUser(noteReport.User);
                
        var instanceLink = linkService.GetLinkToInstance(noteReport.Instance);
        var localInstanceLink = linkService.GetLocalLinkToInstance(noteReport.Instance);
        var localNoteLink = linkService.GetLocalLinkToNote(noteReport.Note);
        var localUserLink = linkService.GetLocalLinkToUser(noteReport.User);
        
        // Note
        list
            .BeginListItem()
                // Note remote link
                .AppendText("Remote note ")
                .BeginLink(noteLink)
                    .AppendCode(noteReport.Note.Id)
                .End()
                    
                // Note local link
                .AppendText(" ")
                .BeginLink(localNoteLink)
                    .AppendItalics("[local mirror]")
                .End()
            .End();
        
        // Note's user
        list
            .BeginList()
                .BeginListItem()
                    // User remote link
                    .AppendText("by user ")
                    .BeginLink(userLink)
                        .AppendCode(noteReport.User.Id)
                        .AppendText($" ({noteReport.User.Username}@{noteReport.User.Host})")
                    .End()
                        
                    // User local link
                    .AppendText(" ")
                    .BeginLink(localUserLink)
                        .AppendItalics("[local mirror]")
                    .End()
                .End()
            .End();
                
        // Note's user's instance
        list
            .BeginList()
                .BeginListItem()
                    // Instance remote link
                    .AppendText("from instance ")
                    .BeginLink(instanceLink)
                        .AppendCode(noteReport.Instance.Id)
                        .AppendText($" ({noteReport.Instance.Host})")
                    .End()

                    // instance local link
                    .AppendText(" ")
                    .BeginLink(localInstanceLink)
                        .AppendItalics("[local mirror]")
                    .End()
                .End()
            .End();
    }

    private static void AppendSectionHeader<T>(SectionBuilder<T> document, int count, string type)
        where T : BuilderBase<T>
    {
        var header = count == 1
            ? $"Flagged 1 {type}:"
            : $"Flagged {count} {type}s:";

        document.AppendHeader(header);
    }

    private static void AppendFlags<T>(ListBuilder<T> list, ReportFlags flags, FlagInclusion includeFlags)
        where T : BuilderBase<T>
    {
        if (includeFlags == FlagInclusion.None)
            return;
        
        if (!flags.HasAny)
            return;
        
        var subList = list.BeginList();

        AppendTextFlags(flags, subList, includeFlags);
        AppendAgeRangeFlags(flags, subList, includeFlags);
        
        subList.End();
    }

    private static void AppendTextFlags<T>(ReportFlags flags, ListBuilder<ListBuilder<T>> subList, FlagInclusion includeFlags)
        where T : BuilderBase<T>
    {
        if (!flags.HasText)
            return;

        foreach (var pair in flags.Text)
        {
            AppendFlagsOfType(subList, pair.Key, pair.Value, includeFlags);
        }
    }

    private static void AppendAgeRangeFlags<T>(ReportFlags flags, ListBuilder<ListBuilder<T>> subList, FlagInclusion includeFlags)
        where T : BuilderBase<T>
    {
        if (!flags.HasAgeRanges)
            return;


        var rangeFlags = new MultiMap<string, Range>();
        foreach (var ageRange in flags.AgeRanges)
        {
            rangeFlags.Add(ageRange.ToString(), Range.All);
        }
        
        AppendFlagsOfType(subList, "age", rangeFlags, includeFlags);
    }

    private static void AppendFlagsOfType<T>(ListBuilder<ListBuilder<T>> subList, string category, MultiMap<string, Range> flags, FlagInclusion includeFlags)
        where T : BuilderBase<T>
    {
        if (flags.Count < 1)
            return;
        
        var item = subList.BeginListItem();
        item.AppendText("for ");
        item.AppendText(category);
        item.AppendText(": ");

        var first = true;
        foreach (var flag in flags.Mappings)
        {
            if (!first)
                item.AppendText(", ");
            first = false;

            if (includeFlags == FlagInclusion.Full)
                item.AppendFullFlaggedText(flag.Key, flag.Value);
            else
                item.AppendMinimalFlaggedText(flag.Key, flag.Value);
        }

        item.End();
    }
}