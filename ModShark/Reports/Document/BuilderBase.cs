using System.Text;

namespace ModShark.Reports.Document;

public abstract class BuilderBase
{
    /// <summary>
    /// The format of text produced from this builder.
    /// </summary>
    public abstract DocumentFormat Format { get; }

    /// <summary>
    /// Returns the total length of the builder's content.
    /// Includes the prefix and suffix.
    /// </summary>
    public int GetLength()
    {
        var total = Elements.Aggregate(0, (sum, e) => sum + e.Length);

        if (Prefix != null)
            total += Prefix.Length;

        if (Suffix != null)
            total += Suffix.Length;

        return total;
    }
    
    /// <summary>
    /// String to prepend before the contents and children.
    /// </summary>
    public abstract string? Prefix { get; }
    
    /// <summary>
    /// String to append after the contents and children.
    /// </summary>
    public abstract string? Suffix { get; }

    /// <summary>
    /// Returns the content of this builder as a list of strings and child builders.
    /// </summary>
    public IReadOnlyList<Element> GetElements() => Elements;
    protected readonly List<Element> Elements = [];
}

public abstract class BuilderBase<TThis> : BuilderBase
    where TThis : BuilderBase<TThis>
{
    protected abstract TThis Self { get; }

    /// <summary>
    /// Appends raw content to this builder.
    /// </summary>
    public TThis Append(string contents)
    {
        Elements.Add(new Element(contents));
        return Self;
    }

    /// <summary>
    /// Appends a child to this builder and returns it.
    /// </summary>
    protected TChild Append<TChild>(TChild childBuilder)
        where TChild : BuilderBase
    {
        Elements.Add(new Element(childBuilder));
        return childBuilder;
    }

    /// <summary>
    /// Appends text w/ a raw prefix and suffix
    /// </summary>
    protected TThis Append(string prefix, string contents, string suffix)
        => Append(prefix).AppendText(contents).Append(suffix);

    /// <summary>
    /// Appends text to the builder
    /// </summary>
    public TThis AppendText(string contents)
    {
        var text = Format.Text(contents);
        return Append(text);
    }

    /// <summary>
    /// Appends text to the builder.
    /// All provided strings are appended in-order
    /// </summary>
    public TThis AppendText(params string[] contents)
    {
        foreach (var text in contents)
        {
            AppendText(text);
        }

        return Self;
    }

    /// <summary>
    /// Appends a single line of text.
    /// No trailing newline is appended.
    /// </summary>
    public TThis AppendInline(string contents)
    {
        var line = Format.TextInline(contents);
        return Append(line);
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        ToString(this, stringBuilder);
        return stringBuilder.ToString();
    }

    private static void ToString(BuilderBase docBuilder, StringBuilder stringBuilder)
    {
        if (docBuilder.Prefix != null)
            stringBuilder.Append(docBuilder.Prefix);

        foreach (var element in docBuilder.GetElements())
        {
            if (element.IsText)
                stringBuilder.Append(element.Text);
            else
                ToString(element.Builder, stringBuilder);
        }

        if (docBuilder.Suffix != null)
            stringBuilder.Append(docBuilder.Suffix);
    }
}