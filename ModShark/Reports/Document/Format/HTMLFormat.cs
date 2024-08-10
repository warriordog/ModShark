namespace ModShark.Reports.Document.Format;

/// <summary>
/// Basic HTML formatting.
/// </summary>
public class HTMLFormat : DocumentFormat
{
    /// <summary>
    /// Value of the anchor's "rel" property.
    /// If null, then rel will be excluded from the output.
    /// </summary>
    public string? LinkRel { get; init; }
    
    /// <summary>
    /// Value of the anchor's "target" property.
    /// If null, then target will be excluded from the output.
    /// </summary>
    public string? LinkTarget { get; init; }
    
    public override string LinkStart(string href) =>
        LinkRel != null
            ? LinkTarget != null
                ? $"<a href=\"{href}\" rel=\"{LinkRel}\" target=\"{LinkTarget}\">"
                : $"<a href=\"{href}\" rel=\"{LinkRel}\">"
            : LinkTarget != null
                ? $"<a href=\"{href}\" target=\"{LinkTarget}\">"
                : $"<a href=\"{href}\">";
    public override string LinkEnd(string href) => "</a>";

    public override string ItalicsStart() => "<span style=\"font-style: italic\">";
    public override string ItalicsEnd() => "</span>";

    public override string BoldStart() => "<span style=\"font-weight: bold\">";
    public override string BoldEnd() => "</span>";

    public override string CodeStart() => "<code>";
    public override string CodeEnd() => "</code>";

    public override string ListStart() => "<ul>";
    public override string ListEnd() => "</ul>";

    public override string ListItemStart() => "<li>";
    public override string ListItemEnd() => "</li>";

    public override string SectionStart() => "<div>";
    public override string SectionEnd() => "</div>";

    public override string HeaderStart() => "<h2>";
    public override string HeaderEnd() => "</h2>";

    public override string TitleStart() => "<h1>";
    public override string TitleEnd() => "</h1>";

    public override string LineBreak() => "<br>";
}