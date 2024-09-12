using System.Diagnostics.CodeAnalysis;

namespace ModShark.Reports.Document;

public readonly struct Element
{
    public BuilderBase? Builder { get; }
    public string? Text { get; }

    [MemberNotNullWhen(true, nameof(Text))]
    [MemberNotNullWhen(false, nameof(Builder))]
    public bool IsText => Text != null;
    
    [MemberNotNullWhen(false, nameof(Text))]
    [MemberNotNullWhen(true, nameof(Builder))]
    public bool IsBuilder => Builder != null;

    public int Length => IsText
        ? Text.Length
        : Builder.GetLength();
    
    public Element(BuilderBase builder) => Builder = builder;
    public Element(string text) => Text = text;
}