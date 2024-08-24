using ModShark.Reports.Document;
using ModShark.Services;

namespace ModShark.Reports.Render;

public interface IRenderService
{
    DocumentBuilder RenderReport(Report report, DocumentFormat format);
}

public class RenderService(ILinkService linkService) : IRenderService
{
    public DocumentBuilder RenderReport(Report report, DocumentFormat format)
    {
        var document = new DocumentBuilder(format);
        document.AppendTitle("ModShark Report");
        
        RenderInstanceReports(document, report);
        RenderUserReports(document, report);
        RenderNoteReports(document, report);

        return document;
    }

    private void RenderInstanceReports(DocumentBuilder document, Report report)
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

    private void RenderUserReports(DocumentBuilder document, Report report)
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
                RenderLocalUserReport(list, userReport);
            else
                RenderRemoteUserReport(list, userReport);
        }
        list.End();

        section.End();
    }

    private void RenderLocalUserReport<T>(ListBuilder<T> list, UserReport userReport)
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
    
    private void RenderRemoteUserReport<T>(ListBuilder<T> list, UserReport userReport)
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

    private void RenderNoteReports(DocumentBuilder document, Report report)
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
                RenderLocalNoteReport(list, noteReport);
            else
                RenderRemoteNoteReport(list, noteReport);
        }
        list.End();

        section.End();
    }

    private void RenderLocalNoteReport<T>(ListBuilder<T> list, NoteReport noteReport)
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

    private void RenderRemoteNoteReport<T>(ListBuilder<T> list, NoteReport noteReport)
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

    private static void RenderSectionHeader<T>(SectionBuilder<T> document, int count, string type)
        where T : BuilderBase<T>
    {
        var header = count == 1
            ? $"Flagged 1 {type}:"
            : $"Flagged {count} {type}s:";

        document.AppendHeader(header);
    }
}