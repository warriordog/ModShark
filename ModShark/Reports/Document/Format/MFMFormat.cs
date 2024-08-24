using ModShark.Utils;

namespace ModShark.Reports.Document.Format;

/// <summary>
/// Misskey-Flavored Markdown formatting.
/// Inline HTML is used to work around common parser bugs.
/// </summary>
public class MFMFormat : MarkdownFormat
{
    // MFM can't be escaped :|
    public override string Text(string text) => text;

    public override string ItalicsStart() => "<i>";
    public override string ItalicsEnd() => "</i>";

    public override string BoldStart() => "<b>";
    public override string BoldEnd() => "</b>";
    
    public override string ListItemStart(int level) => "- ".Indent("  ", level);

    public override string HeaderStart() => BoldStart();
    public override string HeaderEnd() => BoldEnd() + LineBreak();

    public override string TitleStart() => HeaderStart();
    public override string TitleEnd() => HeaderEnd();
}