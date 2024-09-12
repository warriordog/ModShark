namespace ModShark.Reports.Document;

public class ListBuilder<TParent>(string prefix, TParent parent, string suffix, int level) : BuilderBase<ListBuilder<TParent>>
    where TParent : BuilderBase<TParent>
{
    public override string Prefix { get; } = prefix;
    public override string Suffix { get; } = suffix;
    protected override ListBuilder<TParent> Self => this;

    private int Level { get; } = level;

    public override DocumentFormat Format => parent.Format;
    
    public SegmentBuilder<ListBuilder<TParent>> BeginListItem() =>
        Append(
            new SegmentBuilder<ListBuilder<TParent>>(
                Format.ListItemStart(Level),
                this,
                Format.ListItemEnd(Level)
            )
        );
    
    public ListBuilder<TParent> AppendListItem(string contents)  =>
        Append(
            Format.ListItemStart(Level),
            contents,
            Format.ListItemEnd(Level)
        );
    
    public ListBuilder<ListBuilder<TParent>> BeginList() =>
        Append(
            new ListBuilder<ListBuilder<TParent>>(
                Format.SubListStart(Level + 1),
                this,
                Format.SubListEnd(Level + 1),
                Level + 1
            )
        );

    public TParent End() => parent;
}