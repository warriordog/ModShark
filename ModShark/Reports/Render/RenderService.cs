using ModShark.Reports.Document;
using ModShark.Services;

namespace ModShark.Reports.Render;

public interface IRenderService
{
    RenderHints DefaultHints { get; }
    
    DocumentBuilder RenderReport(Report report, DocumentFormat format, RenderHints? hints = null);
}

public class RenderService(ILinkService linkService) : IRenderService
{
    public RenderHints DefaultHints { get; } = new()
    {
        LimitWidth = false
    };
    
    public DocumentBuilder RenderReport(Report report, DocumentFormat format, RenderHints? hints = null)
    {
        hints ??= DefaultHints;
        
        var document = new DocumentBuilder(format);
        document.AppendTitle("ModShark Report");
        
        RenderInstanceReports(document, hints, report);
        RenderUserReports(document, hints, report);
        RenderNoteReports(document, hints, report);

        return document;
    }

    private void RenderInstanceReports(DocumentBuilder document, RenderHints _, Report report)
    {
        if (!report.HasInstanceReports)
            return;

        var section = document.BeginSection();
        
        var count = report.InstanceReports.Count;
        RenderSectionHeader(section, count, "instance");

        var list = section.BeginList();
        foreach (var instanceReport in report.InstanceReports)
        {
            RenderInstanceReport(list, instanceReport);
        }
        list.End();

        section.End();
    }

    private void RenderInstanceReport<T>(ListBuilder<T> list, InstanceReport instanceReport)
        where T : BuilderBase<T>
    {
        var instanceLink = linkService.GetLinkToInstance(instanceReport.Instance);
        var localInstanceLink = linkService.GetLocalLinkToInstance(instanceReport.Instance);
            
        list
            .BeginListItem()
                // Instance remote link
                .Append("Remote instance ")
                .BeginLink(instanceLink)
                    .AppendCode(instanceReport.Instance.Id)
                    .Append(" (", instanceReport.Instance.Host, ")")
                .End()

                // instance local link
                .Append(" ")
                .BeginLink(localInstanceLink)
                    .AppendItalics("[local mirror]")
                .End()
            .End();
    }

    private void RenderUserReports(DocumentBuilder document, RenderHints hints, Report report)
    {
        if (!report.HasUserReports)
            return;
        
        var section = document.BeginSection();
        
        var count = report.UserReports.Count;
        RenderSectionHeader(section, count, "user");

        var list = section.BeginList();
        foreach (var userReport in report.UserReports)
        {
            
            if (userReport.IsLocal)
                RenderLocalUserReport(list, hints, userReport);
            else
                RenderRemoteUserReport(list, hints, userReport);
        }
        list.End();

        section.End();
    }

    private void RenderLocalUserReport<T>(ListBuilder<T> list, RenderHints _, UserReport userReport)
        where T : BuilderBase<T>
    {
        if (!userReport.IsLocal)
            return;
        
        var userLink = linkService.GetLinkToUser(userReport.User);

        // User local link
        list
            .BeginListItem()
                .BeginBold()
                    .Append("Local user ")
                    .BeginLink(userLink)
                        .AppendCode(userReport.User.Id)
                        .Append($" ({userReport.User.Username})")
                    .End()
                .End()
            .End();
    }
    
    private void RenderRemoteUserReport<T>(ListBuilder<T> list, RenderHints hints, UserReport userReport)
        where T : BuilderBase<T>
    {
        if (userReport.IsLocal)
            return;
        
        var userLink = linkService.GetLinkToUser(userReport.User);
        var instanceLink = linkService.GetLinkToInstance(userReport.Instance);
        var localInstanceLink = linkService.GetLocalLinkToInstance(userReport.Instance);
        var localUserLink = linkService.GetLocalLinkToUser(userReport.User);

        var item = list.BeginListItem();
        
        // User
        item
            // User remote link
            .Append("Remote user ")
                .BeginLink(userLink)
                    .AppendCode(userReport.User.Id)
                .Append($" ({userReport.User.Username}@{userReport.Instance.Host})")
            .End()
                    
            // User local link
            .Append(" ")
                .BeginLink(localUserLink)
                .AppendItalics("[local mirror]")
            .End();

        item = NextItemForLine(list, hints, item);
            
        // User's instance
        item
            // Instance remote link
            .Append("from instance ")
            .BeginLink(instanceLink)
                .AppendCode(userReport.Instance.Id)
                .Append($" ({userReport.Instance.Host})")
            .End()

            // instance local link
            .Append(" ")
            .BeginLink(localInstanceLink)
                .AppendItalics("[local mirror]")
            .End();
                
        item.End();
    }

    private void RenderNoteReports(DocumentBuilder document, RenderHints hints, Report report)
    {
        if (!report.HasNoteReports)
            return;
        
        var section = document.BeginSection();
        
        var count = report.NoteReports.Count;
        RenderSectionHeader(section, count, "note");

        var list = section.BeginList();
        foreach (var noteReport in report.NoteReports)
        {
            if (noteReport.IsLocal)
                RenderLocalNoteReport(list, hints, noteReport);
            else
                RenderRemoteNoteReport(list, hints, noteReport);
        }
        list.End();

        section.End();
    }

    private void RenderLocalNoteReport<T>(ListBuilder<T> list, RenderHints hints, NoteReport noteReport)
        where T : BuilderBase<T>
    {
        if (!noteReport.IsLocal)
            return;
        
        var noteLink = linkService.GetLinkToNote(noteReport.Note);
        var userLink = linkService.GetLinkToUser(noteReport.User);

        var item = list.BeginListItem();
        
        // note local link
        item
            .BeginBold()
                .Append("Local note ")
                    .BeginLink(noteLink)
                    .AppendCode(noteReport.Note.Id)
                .End()
            .End();

        item = NextItemForLine(list, hints, item);
                
        // user local link
        item
            .Append("by user ")
            .BeginLink(userLink)
                .AppendCode(noteReport.User.Id)
                .Append($" ({noteReport.User.Username})")
            .End();
            
        item.End();
    }

    private void RenderRemoteNoteReport<T>(ListBuilder<T> list, RenderHints hints, NoteReport noteReport)
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
                
        var item = list.BeginListItem();
        
        // Note
        item
            // Note remote link
            .Append("Remote note ")
            .BeginLink(noteLink)
                .AppendCode(noteReport.Note.Id)
            .End()
                
            // Note local link
            .Append(" ")
            .BeginLink(localNoteLink)
                .AppendItalics("[local mirror]")
            .End();

        item = NextItemForLine(list, hints, item);
        
        // Note's user
        item
            // User remote link
            .Append("by user ")
            .BeginLink(userLink)
                .AppendCode(noteReport.User.Id)
                .Append($" ({noteReport.User.Username}@{noteReport.User.Host})")
            .End()
                
            // User local link
            .Append(" ")
            .BeginLink(localUserLink)
                .AppendItalics("[local mirror]")
            .End();

        item = NextItemForLine(list, hints, item);
                
        // Note's user's instance
        item
            // Instance remote link
            .Append("from instance ")
            .BeginLink(instanceLink)
                .AppendCode(noteReport.Instance.Id)
                .Append($" ({noteReport.Instance.Host})")
            .End()

            // instance local link
            .Append(" ")
            .BeginLink(localInstanceLink)
                .AppendItalics("[local mirror]")
            .End();
                
        item.End();
    }

    private static SegmentBuilder<ListBuilder<T>> NextItemForLine<T>(ListBuilder<T> list, RenderHints hints, SegmentBuilder<ListBuilder<T>> item)
        where T : BuilderBase<T>
    {
        if (hints.LimitWidth)
        {
            item.End();
            return list.BeginListItem();
        }
            
        return item.Append(" ");
    }

    private static void RenderSectionHeader<T>(SectionBuilder<T> document, int count, string type)
        where T : BuilderBase<T>
    {
        var header = count == 1
            ? $"Flagged 1 {type}:"
            : $"Flagged {count} {type}s:";

        document.AppendHeader(header);
    }
}