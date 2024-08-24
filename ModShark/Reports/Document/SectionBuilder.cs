namespace ModShark.Reports.Document;

public class SectionBuilder<TBuilder> : SectionBase<SectionBuilder<TBuilder>>
    where TBuilder : BuilderBase<TBuilder>
{
    private SectionBase<TBuilder> Builder { get; }
    private string Suffix { get; }

    public override DocumentFormat Format => Builder.Format;
    
    public SectionBuilder(string prefix, SectionBase<TBuilder> builder, string suffix)
    {
        Builder = builder;
        Suffix = suffix;
        
        Builder.Append(prefix);
    }

    public override SectionBuilder<TBuilder> Append(string contents)
    {
        Builder.Append(contents);
        return this;
    }

    public override SectionBuilder<TBuilder> Append(params string[] contents)
    {
        Builder.Append(contents);
        return this;
    }
    
    public SectionBuilder<TBuilder> AppendHeader(string contents)  =>
        AppendText(
            Format.HeaderStart(),
            contents,
            Format.HeaderEnd()
        );
    
    public SegmentBuilder<SectionBuilder<TBuilder>> BeginHeader() =>
        new(
            Format.HeaderStart(),
            this,
            Format.HeaderEnd()
        );
    
    public TBuilder End()
        => Builder.Append(Suffix);
}