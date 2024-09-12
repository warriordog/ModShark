namespace ModShark.Reports.Document;

public class SectionBuilder<TParent>(string? prefix, TParent parent, string? suffix) : SectionBase<SectionBuilder<TParent>>(prefix, suffix)
    where TParent : BuilderBase<TParent>
{
    protected override SectionBuilder<TParent> Self => this;

    public override DocumentFormat Format => parent.Format;
    
    public SectionBuilder<TParent> AppendHeader(string contents)  =>
        Append(
            Format.HeaderStart(),
            contents,
            Format.HeaderEnd()
        );
    
    public SegmentBuilder<SectionBuilder<TParent>> BeginHeader() =>
        Append(
            new SegmentBuilder<SectionBuilder<TParent>>(
                Format.HeaderStart(),
                this,
                Format.HeaderEnd()
            )
        );
    
    public TParent End() => parent;
    
    /// <summary>
    /// Creates a logical group of related elements that should be rendered together.
    /// This does not change the generated output, but will ensure that grouped lines wrap together.
    /// </summary>    
    public SectionBuilder<SectionBuilder<TParent>> BeginGroup() =>
        Append(
            new SectionBuilder<SectionBuilder<TParent>>(null, Self, null)
        );
}