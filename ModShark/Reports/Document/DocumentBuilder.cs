using System.Text;

namespace ModShark.Reports.Document;

/// <summary>
/// Format-agnostic and semantics-aware text builder for constructing long-form documents.
/// The resulting output can be chunked into syntactically-correct segments of any length.
/// </summary>
/// <remarks>
/// This implementation does not attempt to sanitize or filter the input.
/// Ensure that all string values are trusted or externally validated!
/// </remarks>
public class DocumentBuilder(DocumentFormat format) : SectionBase<DocumentBuilder>
{
    public override DocumentFormat Format { get; } = format;
    
    public override string? Prefix => null;
    public override string? Suffix => null;
    protected override DocumentBuilder Self => this;
    
    public DocumentBuilder AppendTitle(string contents)  =>
        Append(
            Format.TitleStart(),
            contents,
            Format.TitleEnd()
        );
    
    public SegmentBuilder<DocumentBuilder> BeginTitle() =>
        Append(
            new SegmentBuilder<DocumentBuilder>(
                Format.TitleStart(),
                this,
                Format.TitleEnd()
            )
        );
    
    public DocumentBuilder AppendSection(string contents)  =>
        Append(
            Format.SectionStart(),
            contents,
            Format.SectionEnd()
        );
    
    public SectionBuilder<DocumentBuilder> BeginSection() =>
        Append(
            new SectionBuilder<DocumentBuilder>(
                Format.SectionStart(),
                this,
                Format.SectionEnd()
            )
        );

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        ToStrings(int.MaxValue, [], this, ref stringBuilder);
        return stringBuilder.ToString();
    }

    public List<string> ToStrings(int maxLength)
    {
        if (maxLength < 1)
            throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength, "max length must be a positive integer");

        var blocks = new List<string>();
        var stringBuilder = new StringBuilder();

        // Serialize into blocks
        ToStrings(maxLength, blocks, this, ref stringBuilder);
        
        // Make sure we get the last partial segment
        if (stringBuilder.Length > 0)
            blocks.Add(stringBuilder.ToString());

        return blocks;
    }
    
    private static void ToStrings(int maxLength, List<string> blocks, BuilderBase docBuilder, ref StringBuilder stringBuilder)
    {
        // 1. If the builder doesn't fit in the remaining space, but *could* fit in an empty block, then break.
        var docLength = docBuilder.GetLength();
        var nextLength = stringBuilder.Length + docLength;
        if (nextLength > maxLength && docLength <= maxLength)
        {
            var block = stringBuilder.ToString();
            blocks.Add(block);
            
            stringBuilder = new StringBuilder();
        }
        
        // 2. Write prefix
        if (docBuilder.Prefix != null)
            AppendString(maxLength, blocks, docBuilder.Prefix, ref stringBuilder);
        
        // 3. Write contents.
        foreach (var child in docBuilder.GetElements())
        {
            // Append text directly
            if (child.IsText)
                AppendString(maxLength, blocks, child.Text, ref stringBuilder);
            
            // Recursively append other builders
            else
                ToStrings(maxLength, blocks, child.Builder, ref stringBuilder);
        }
        
        // 4. Write suffix
        if (docBuilder.Suffix != null)
            AppendString(maxLength, blocks, docBuilder.Suffix, ref stringBuilder);
    }

    private static void AppendString(int maxLength, List<string> blocks, string text, ref StringBuilder stringBuilder)
    {
        // Wrap if the string won't fit
        var nextLength = stringBuilder.Length + text.Length;
        if (nextLength > maxLength)
        {
            var block = stringBuilder.ToString();
            blocks.Add(block);
            
            stringBuilder = new StringBuilder();
        }

        // Append to whichever builder is up
        stringBuilder.Append(text);
    }
}