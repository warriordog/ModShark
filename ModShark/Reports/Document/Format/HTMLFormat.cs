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

    // https://stackoverflow.com/a/7382028
    public override string Text(string text) => text
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\"", "&quot;")
        .Replace("'", "&#39;");

    public override string LinkStart(string href) =>
        LinkRel != null
            ? LinkTarget != null
                ? $"<a href=\"{Text(href)}\" rel=\"{Text(LinkRel)}\" target=\"{Text(LinkTarget)}\">"
                : $"<a href=\"{Text(href)}\" rel=\"{Text(LinkRel)}\">"
            : LinkTarget != null
                ? $"<a href=\"{Text(href)}\" target=\"{Text(LinkTarget)}\">"
                : $"<a href=\"{Text(href)}\">";
    public override string LinkEnd(string href) => "</a>";

    public override string ItalicsStart() => "<span style=\"font-style: italic\">";
    public override string ItalicsEnd() => "</span>";

    public override string BoldStart() => "<span style=\"font-weight: bold\">";
    public override string BoldEnd() => "</span>";

    public override string CodeStart() => "<code>";
    public override string CodeEnd() => "</code>";

    public override string ListStart() => "<ul>";
    public override string ListEnd() => "</ul>";

    public override string ListItemStart(int level) => "<li>";
    public override string ListItemEnd(int level) => "</li>";

    public override string SubListStart(int level) => ListItemStart(level) + ListStart();
    public override string SubListEnd(int level) => ListEnd() + ListItemEnd(level);

    public override string SectionStart() => "<div>";
    public override string SectionEnd() => "</div>";

    public override string HeaderStart() => "<h2>";
    public override string HeaderEnd() => "</h2>";

    public override string TitleStart() => "<h1>";
    public override string TitleEnd() => "</h1>";
    
    // HTML3 (as used in email) does not support any kind of spoiler effect
    public override string SpoilerStart(string placeholder) => "";
    public override string SpoilerEnd(string placeholder) => "";

    public override string LineBreak() => "<br>";
}