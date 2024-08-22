﻿using ModShark.Utils;

namespace ModShark.Reports.Document.Format;

/// <summary>
/// Standard Markdown formatting.
/// </summary>
public class MarkdownFormat : DocumentFormat
{
    public override string LinkStart(string href) => "[";
    public override string LinkEnd(string href) => $"]({href})";

    public override string ItalicsStart() => "*";
    public override string ItalicsEnd() => "*";

    public override string BoldStart() => "**";
    public override string BoldEnd() => "**";

    public override string CodeStart() => "`";
    public override string CodeEnd() => "`";

    public override string ListStart() => LineBreak();
    public override string ListEnd() => "";

    public override string ListItemStart(int level) => "* ".Indent("  ", level);
    public override string ListItemEnd(int level) => LineBreak();

    public override string SubListStart(int level) => "";
    public override string SubListEnd(int level) => "";

    public override string SectionStart() => LineBreak();
    public override string SectionEnd() => LineBreak();

    public override string HeaderStart() => "## ";
    public override string HeaderEnd() => LineBreak();

    public override string TitleStart() => "# ";
    public override string TitleEnd() => LineBreak();

    public override string LineBreak() => "\n";
}