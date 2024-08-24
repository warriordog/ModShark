using ModShark.Reports.Document;
using ModShark.Services;

namespace ModShark.Reports.Render;

public interface IRenderService
{
    DocumentBuilder RenderReport(Report report, DocumentFormat format, bool includeFlags = true);
}

public class RenderService(ILinkService linkService) : IRenderService
{
    public DocumentBuilder RenderReport(Report report, DocumentFormat format, bool includeFlags = true)
    {
        var document = new DocumentBuilder(format);
        document.AppendTitle("ModShark Report");
        
        RenderInstanceReports(document, report, includeFlags);
        RenderUserReports(document, report, includeFlags);
        RenderNoteReports(document, report, includeFlags);

        return document;
    }

    private void RenderInstanceReports(DocumentBuilder document, Report report, bool includeFlags)
    {
        if (!report.HasInstanceReports)
            return;

        var section = document.BeginSection();
        
        var count = report.InstanceReports.Count;
        AppendSectionHeader(section, count, "instance");

        var list = section.BeginList();
        foreach (var instanceReport in report.InstanceReports)
        {
            AppendInstanceReport(list, instanceReport);
            
            if (includeFlags)
                AppendFlags(list, instanceReport.Flags);
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

    private void RenderUserReports(DocumentBuilder document, Report report, bool includeFlags)
    {
        if (!report.HasUserReports)
            return;
        
        var section = document.BeginSection();
        
        var count = report.UserReports.Count;
        AppendSectionHeader(section, count, "user");

        var list = section.BeginList();
        foreach (var userReport in report.UserReports)
        {
            
            if (userReport.IsLocal)
                AppendLocalUserReport(list, userReport);
            else
                AppendRemoteUserReport(list, userReport);

            if (includeFlags)
                AppendFlags(list, userReport.Flags);
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

    private void RenderNoteReports(DocumentBuilder document, Report report, bool includeFlags)
    {
        if (!report.HasNoteReports)
            return;
        
        var section = document.BeginSection();
        
        var count = report.NoteReports.Count;
        AppendSectionHeader(section, count, "note");

        var list = section.BeginList();
        foreach (var noteReport in report.NoteReports)
        {
            if (noteReport.IsLocal)
                AppendLocalNoteReport(list, noteReport);
            else
                AppendRemoteNoteReport(list, noteReport);

            if (includeFlags)
                AppendFlags(list, noteReport.Flags);
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

    private static void AppendFlags<T>(ListBuilder<T> list, ReportFlags flags) where T : BuilderBase<T>
    {
        if (!flags.HasAny)
            return;
        
        var subList = list.BeginList();

        AppendTextFlags(flags, subList);
        AppendAgeRangeFlags(flags, subList);
        
        subList.End();
    }

    private static void AppendTextFlags<T>(ReportFlags flags, ListBuilder<ListBuilder<T>> subList) where T : BuilderBase<T>
    {
        if (!flags.HasText)
            return;
        
        var item = subList.BeginListItem();
        item.Append("for text: ");

        var first = true;
        foreach (var text in flags.Text)
        {
            if (!first)
                item.AppendText(", ");
            first = false;

            item.AppendCode(text);
        }

        item.End();
    }

    private static void AppendAgeRangeFlags<T>(ReportFlags flags, ListBuilder<ListBuilder<T>> subList) where T : BuilderBase<T>
    {
        if (!flags.HasAgeRanges)
            return;
        
        var item = subList.BeginListItem();
        item.Append("for age: ");
            
        var first = true;
        foreach (var ageRange in flags.AgeRanges)
        {
            if (!first)
                item.AppendText(", ");
            first = false;

            item.AppendCode(ageRange.ToString());
        }

        item.End();
    }
}