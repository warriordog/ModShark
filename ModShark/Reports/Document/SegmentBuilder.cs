namespace ModShark.Reports.Document;

public class SegmentBuilder<TParent>(string prefix, TParent builder, string suffix) : SegmentBase<SegmentBuilder<TParent>>
    where TParent : BuilderBase<TParent>
{
    protected override SegmentBuilder<TParent> Self => this;
    
    public override string Prefix { get; } = prefix;
    public override string Suffix { get; } = suffix;

    public override DocumentFormat Format => builder.Format;

    public TParent End() => builder;
}