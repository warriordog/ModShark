﻿namespace ModShark.Reports.Document;

public abstract class SegmentBase<TBuilder> : BuilderBase<TBuilder>
    where TBuilder : BuilderBase<TBuilder>
{
    public TBuilder AppendLink(string href, string contents) =>
        AppendText(
            Format.LinkStart(href),
            contents,
            Format.LinkEnd(href)
        );
    
    public SegmentBuilder<TBuilder> BeginLink(string href) =>
        new(
            Format.LinkStart(href),
            this,
            Format.LinkEnd(href)
        );
    
    public TBuilder AppendItalics(string contents) =>
        AppendText(
            Format.ItalicsStart(),
            contents,
            Format.ItalicsEnd()
        );
    
    public SegmentBuilder<TBuilder> BeginItalics() =>
        new(
            Format.ItalicsStart(),
            this,
            Format.ItalicsEnd()
        );
    
    public TBuilder AppendBold(string contents) =>
        AppendText(
            Format.BoldStart(),
            contents,
            Format.BoldEnd()
        );

    public SegmentBuilder<TBuilder> BeginBold() =>
        new(
            Format.BoldStart(),
            this,
            Format.BoldEnd()
        );
    
    public TBuilder AppendCode(string contents) =>
        AppendText(
            Format.CodeStart(),
            contents,
            Format.CodeEnd()
        );
    
    public SegmentBuilder<TBuilder> BeginCode() =>
        new(
            Format.CodeStart(),
            this,
            Format.CodeEnd()
        );
}