using System.Text;

namespace ModShark.Utils;

public static class StringBuilderExtensions
{
    public static StringBuilder AppendHtmlAnchor(this StringBuilder builder, string href, Action contents)
        => builder.AppendBlock($"<a href=\"{href}\" rel=\"noopener noreferer\" target=\"_blank\">", "</a>", contents);

    public static StringBuilder AppendHtmlStyle(this StringBuilder builder, string style, Action contents)
        => builder.AppendBlock($"<span style=\"{style}\">", "</span>", contents);

    public static StringBuilder AppendHtml(this StringBuilder builder, string tag, Action contents)
        => builder.AppendHtml(tag, null, contents);
    
    public static StringBuilder AppendHtml(this StringBuilder builder, string tag, string? attributes, Action contents)
    {
        builder.Append($"<{tag}");
        if (attributes != null)
        {
            builder.Append(' ');
            builder.Append(attributes);
        }
        builder.Append('>');

        contents();

        builder.Append($"</{tag}>");
        return builder;
    }

    public static StringBuilder AppendMarkdownItalics(this StringBuilder builder, Action contents)
        => builder.AppendBlock("*", "*", contents);
    
    public static StringBuilder AppendMarkdownItalics(this StringBuilder builder, string contents)
        => builder.AppendBlock("*", "*", contents);
    
    public static StringBuilder AppendMarkdownBold(this StringBuilder builder, Action contents)
        => builder.AppendBlock("**", "**", contents);
    
    public static StringBuilder AppendMarkdownBold(this StringBuilder builder, string contents)
        => builder.AppendBlock("**", "**", contents);

    public static StringBuilder AppendMarkdownCode(this StringBuilder builder, Action contents)
        => builder.AppendBlock("`", "`", contents);
    public static StringBuilder AppendMarkdownCode(this StringBuilder builder, string contents)
        => builder.AppendBlock("`", "`", contents);

    public static StringBuilder AppendMarkdownLink(this StringBuilder builder, string href, Action contents)
        => builder.AppendBlock("[", $"]({href})", contents);
    
    public static StringBuilder AppendMarkdownLink(this StringBuilder builder, string href, string contents)
        => builder.AppendBlock("[", $"]({href})", contents);

    public static StringBuilder AppendMarkdownLinkWithBrackets(this StringBuilder builder, string href, Action contents)
        => builder.AppendBlock("$[scale [][", $"]({href})]", contents);
    
    public static StringBuilder AppendMarkdownLinkWithBrackets(this StringBuilder builder, string href, string contents)
        => builder.AppendBlock("$[scale [][", $"]({href})]", contents);
    
    private static StringBuilder AppendBlock(this StringBuilder builder, string start, string end, Action contents)
    {
        builder.Append(start);
        contents();
        builder.Append(end);

        return builder;
    }
    
    private static StringBuilder AppendBlock(this StringBuilder builder, string start, string end, string contents)
    {
        builder.Append(start);
        builder.Append(contents);
        builder.Append(end);

        return builder;
    }
}