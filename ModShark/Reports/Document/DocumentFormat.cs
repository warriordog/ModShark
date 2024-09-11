using ModShark.Reports.Document.Format;

namespace ModShark.Reports.Document;

/// <summary>
/// Defines a schema for converting raw text input into formatted output.
/// Static properties contain default implementations of standard formats.
/// </summary>
public abstract class DocumentFormat
{
    public static DocumentFormat Markdown { get; } = new MarkdownFormat();
    public static DocumentFormat MFM { get; } = new MFMFormat();
    public static DocumentFormat HTML { get; } = new HTMLFormat();
    public static DocumentFormat HTML5 { get; } = new HTML5Format();

    public abstract string Text(string text);
    public abstract string TextInline(string line);
    
    public abstract string LinkStart(string href);
    public abstract string LinkEnd(string href);

    public abstract string ItalicsStart();
    public abstract string ItalicsEnd();

    public abstract string BoldStart();
    public abstract string BoldEnd();

    public abstract string CodeStart();
    public abstract string CodeEnd();

    public abstract string ListStart();
    public abstract string ListEnd();

    public abstract string ListItemStart(int level);
    public abstract string ListItemEnd(int level);

    public abstract string SubListStart(int level);
    public abstract string SubListEnd(int level);

    public abstract string SectionStart();
    public abstract string SectionEnd();

    public abstract string HeaderStart();
    public abstract string HeaderEnd();

    public abstract string TitleStart();
    public abstract string TitleEnd();

    public abstract string SpoilerStart(string placeholder);
    public abstract string SpoilerEnd(string placeholder);

    public abstract string LineBreak();
}