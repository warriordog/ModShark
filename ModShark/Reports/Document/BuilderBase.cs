namespace ModShark.Reports.Document;

public abstract class BuilderBase<TBuilder>
    where TBuilder : BuilderBase<TBuilder>
{
    public abstract DocumentFormat Format { get; }
    
    public abstract TBuilder Append(string contents);
    public abstract TBuilder Append(params string[] contents);

    public TBuilder AppendText(string contents)
    {
        var text = Format.Text(contents);
        return Append(text);
    }

    public TBuilder AppendText(params string[] contents)
    {
        var texts = contents
            .Select(c => Format.Text(c))
            .ToArray();
        return Append(texts);
    }
}