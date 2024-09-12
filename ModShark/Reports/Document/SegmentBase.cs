namespace ModShark.Reports.Document;

public abstract class SegmentBase<TThis> : BuilderBase<TThis>
    where TThis : BuilderBase<TThis>
{
    public TThis AppendLink(string href, string contents) =>
        Append(
            Format.LinkStart(href),
            contents,
            Format.LinkEnd(href)
        );
    
    public SegmentBuilder<TThis> BeginLink(string href) =>
        Append(
            new SegmentBuilder<TThis>(
                Format.LinkStart(href),
                Self,
                Format.LinkEnd(href)
            )
        );
    
    public TThis AppendItalics(string contents) =>
        Append(
            Format.ItalicsStart(),
            contents,
            Format.ItalicsEnd()
        );
    
    public SegmentBuilder<TThis> BeginItalics() =>
        Append(
            new SegmentBuilder<TThis>(
                Format.ItalicsStart(),
                Self,
                Format.ItalicsEnd()
            )
        );
    
    public TThis AppendBold(string contents) =>
        Append(
            Format.BoldStart(),
            contents,
            Format.BoldEnd()
        );

    public SegmentBuilder<TThis> BeginBold() =>
        Append(
            new SegmentBuilder<TThis>(
                Format.BoldStart(),
                Self,
                Format.BoldEnd()
            )
        );
    
    public TThis AppendCode(string contents) =>
        Append(
            Format.CodeStart(),
            contents,
            Format.CodeEnd()
        );
    
    public SegmentBuilder<TThis> BeginCode() =>
        Append(
            new SegmentBuilder<TThis> (
                Format.CodeStart(),
                Self,
                Format.CodeEnd()
            )
        );
    
    public TThis AppendSpoiler(string contents, string placeholder = "spoiler") =>
        Append(
            Format.SpoilerStart(placeholder),
            contents,
            Format.SpoilerEnd(placeholder)
        );
    
    public SegmentBuilder<TThis> BeginSpoiler(string placeholder = "spoiler") =>
        Append(
            new SegmentBuilder<TThis> (
                Format.SpoilerStart(placeholder),
                Self,
                Format.SpoilerEnd(placeholder)
            )
        );
}