namespace ModShark.Reports.Document;

public abstract class BuilderBase<TBuilder>
    where TBuilder : BuilderBase<TBuilder>
{
    public abstract DocumentFormat Format { get; }
    
    public abstract TBuilder Append(string contents);
    public abstract TBuilder Append(params string[] contents);
}