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
    private List<string> Document { get; } = [];
    public override DocumentFormat Format { get; } = format;


    public override DocumentBuilder Append(string contents)
    {
        Document.Add(contents);
        return this;
    }
    
    public override DocumentBuilder Append(params string[] contents)
    {
        var segment = string.Join("", contents);
        Document.Add(segment);
        return this;
    }
    
    public DocumentBuilder AppendTitle(string contents)  =>
        AppendText(
            Format.TitleStart(),
            contents,
            Format.TitleEnd()
        );
    
    public SegmentBuilder<DocumentBuilder> BeginTitle() =>
        new(
            Format.TitleStart(),
            this,
            Format.TitleEnd()
        );
    
    public DocumentBuilder AppendSection(string contents)  =>
        AppendText(
            Format.SectionStart(),
            contents,
            Format.SectionEnd()
        );
    
    public SectionBuilder<DocumentBuilder> BeginSection() =>
        new(
            Format.SectionStart(),
            this,
            Format.SectionEnd()
        );

    public override string ToString() =>
        string.Join("", Document);

    public IEnumerable<string> ToStrings(int maxLength)
    {
        if (maxLength < 1)
            throw new ArgumentOutOfRangeException(nameof(maxLength), maxLength, "max length must be a positive integer");
        
        var builder = new StringBuilder();

        // Append each segment, yielding each time we reach the max length
        foreach (var segment in Document)
        {
            var nextLength = builder.Length + segment.Length;
            if (nextLength > maxLength)
            {
                yield return builder.ToString();
                builder = new StringBuilder();
            }

            builder.Append(segment);
        }

        // Make sure we get the last partial segment
        if (builder.Length > 0)
        {
            yield return builder.ToString();
        }
    }
}