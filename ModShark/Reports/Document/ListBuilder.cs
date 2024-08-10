namespace ModShark.Reports.Document;

public class ListBuilder<TBuilder> : BuilderBase<ListBuilder<TBuilder>>
    where TBuilder : BuilderBase<TBuilder>
{
    private BuilderBase<TBuilder> Builder { get; }
    private string Suffix { get; }

    public override DocumentFormat Format => Builder.Format;

    public ListBuilder(string prefix, BuilderBase<TBuilder> builder, string suffix)
    {
        Builder = builder;
        Suffix = suffix;
        
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
            Format.ListItemStart(),
            this,
            Format.ListItemEnd()
        );
    
    public ListBuilder<TBuilder> AppendListItem(string contents)  =>
        Append(
            Format.ListItemStart(),
            contents,
            Format.ListItemEnd()
        );
    
    public TBuilder End()
        => Builder.Append(Suffix);
}