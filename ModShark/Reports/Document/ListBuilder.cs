namespace ModShark.Reports.Document;

public class ListBuilder<TBuilder> : BuilderBase<ListBuilder<TBuilder>>
    where TBuilder : BuilderBase<TBuilder>
{
    private BuilderBase<TBuilder> Builder { get; }
    private string Suffix { get; }
    private int Level { get; }

    public override DocumentFormat Format => Builder.Format;
    
    public ListBuilder(string prefix, BuilderBase<TBuilder> builder, string suffix, int level)
    {
        Builder = builder;
        Suffix = suffix;
        Level = level;
        
        Builder.Append(prefix);
    }

    public override ListBuilder<TBuilder> Append(string contents)
    {
        Builder.Append(contents);
        return this;
    }

    public override ListBuilder<TBuilder> Append(params string[] contents)
    {
        Builder.Append(contents);
        return this;
    }
    
    public SegmentBuilder<ListBuilder<TBuilder>> BeginListItem() =>
        new(
            Format.ListItemStart(Level),
            this,
            Format.ListItemEnd(Level)
        );
    
    public ListBuilder<TBuilder> AppendListItem(string contents)  =>
        Append(
            Format.ListItemStart(Level),
            contents,
            Format.ListItemEnd(Level)
        );
    
    public ListBuilder<ListBuilder<TBuilder>> BeginList() =>
        new(
            Format.SubListStart(Level + 1),
            this,
            Format.SubListEnd(Level + 1),
            Level + 1
        );
    
    public TBuilder End()
        => Builder.Append(Suffix);
}