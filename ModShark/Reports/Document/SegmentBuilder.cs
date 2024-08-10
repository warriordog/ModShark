using System.Text;

namespace ModShark.Reports.Document;

public class SegmentBuilder<TBuilder> : SegmentBase<SegmentBuilder<TBuilder>>
    where TBuilder : BuilderBase<TBuilder>
{

    private StringBuilder Segment { get; } = new();
    private BuilderBase<TBuilder> Builder { get; }
    private string Suffix { get; }

    public override DocumentFormat Format => Builder.Format;
    
    public SegmentBuilder(string prefix, BuilderBase<TBuilder> builder, string suffix)
    {
        Builder = builder;
        Suffix = suffix;
        
        Segment.Append(prefix);
    }
    
    public override SegmentBuilder<TBuilder> Append(string contents)
    {
        Segment.Append(contents);
        return this;
    }

    public override SegmentBuilder<TBuilder> Append(params string[] contents)
    {
        Segment.AppendJoin("", contents);
        return this;
    }
    
    public TBuilder End()
    {
        Segment.Append(Suffix);

        var segmentContents = Segment.ToString();
        return Builder.Append(segmentContents);
    }
}