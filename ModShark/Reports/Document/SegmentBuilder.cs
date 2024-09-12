namespace ModShark.Reports.Document;

public class SegmentBuilder<TParent>(string? prefix, TParent builder, string? suffix) : SegmentBase<SegmentBuilder<TParent>>(prefix, suffix)
    where TParent : BuilderBase<TParent>
{
    protected override SegmentBuilder<TParent> Self => this;

    public override DocumentFormat Format => builder.Format;

    public TParent End() => builder;
    
    /// <summary>
    /// Creates a logical group of related elements that should be rendered together.
    /// This does not change the generated output, but will ensure that grouped lines wrap together.
    /// </summary>    
    public SegmentBuilder<SegmentBuilder<TParent>> BeginGroup() =>
        Append(
            new SegmentBuilder<SegmentBuilder<TParent>>(null, Self, null)
        );
}