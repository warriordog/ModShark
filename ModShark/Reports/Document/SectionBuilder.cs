namespace ModShark.Reports.Document;

public class SectionBuilder<TParent>(string prefix, TParent parent, string suffix) : SectionBase<SectionBuilder<TParent>>
    where TParent : BuilderBase<TParent>
{
    protected override SectionBuilder<TParent> Self => this;
    
    public override string Prefix { get; } = prefix;
    public override string Suffix { get; } = suffix;

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
}