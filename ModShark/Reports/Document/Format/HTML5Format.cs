namespace ModShark.Reports.Document.Format;

/// <summary>
/// Extension of <see cref="HTMLFormat"/> with support for HTML5 semantic tags.
/// </summary>
public class HTML5Format : HTMLFormat
{
    public override string SectionStart() => "<section>";
    public override string SectionEnd() => "</section>";
}