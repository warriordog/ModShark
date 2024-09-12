namespace ModShark.Reports.Document;

public abstract class SectionBase<TThis> : SegmentBase<TThis>
    where TThis : SectionBase<TThis>
{
    public ListBuilder<TThis> BeginList() =>
        Append(
            new ListBuilder<TThis>(
                Format.ListStart(),
                Self,
                Format.ListEnd(),
                0
            )
        );

    public TThis AppendLineBreak() =>
        Append(Format.LineBreak());
}