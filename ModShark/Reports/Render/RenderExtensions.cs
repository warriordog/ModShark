using ModShark.Reports.Document;

namespace ModShark.Reports.Render;

public static class RenderExtensions
{
    public static TBuilder AppendFlaggedText<TBuilder>(this TBuilder builder, string text, IReadOnlyCollection<Range> ranges)
        where TBuilder : SegmentBase<TBuilder>
    {
        var segments = SplitText(text, ranges);
        
        // Segments are interleaved (text,flag)
        var isFlag = false;
        foreach (var segment in segments)
        {
            if (segment.Length > 0)
            {
                if (isFlag)
                    AppendInlineFlag(builder, segment);
                else
                    AppendInlineText(builder, segment);
            }

            isFlag = !isFlag;
        }

        return builder;
    }

    private static void AppendInlineFlag<TBuilder>(TBuilder builder, string segment) where TBuilder : SegmentBase<TBuilder>
    {
        builder
            .BeginBold()
                .BeginCode()
                    .AppendInline(segment)
                .End()
            .End();
    }

    private static void AppendInlineText<TBuilder>(TBuilder builder, string segment) where TBuilder : SegmentBase<TBuilder>
    {
        builder
            .BeginCode()
                .AppendInline(segment)
            .End();
    }

    private static IEnumerable<string> SplitText(string text, IReadOnlyCollection<Range> ranges)
    {
        if (text.Length < 1)
            yield break;
        
        var rangeStart = 0;
        var rangeIsFlag = IsInRange(0, ranges);
        
        // Make sure we always start with plain text
        if (rangeIsFlag)
            yield return "";
        
        // Assign each character to a segment (text or flag)
        for (var i = 1; i < text.Length; i++)
        {
            var isFlag = IsInRange(i, ranges);
            if (isFlag == rangeIsFlag)
                continue;
         
            // Cut the next segment when it ends   
            yield return text[rangeStart..i];
            rangeStart = i;
            rangeIsFlag = isFlag;
        }
        
        // Handle the last segment
        if (text.Length - rangeStart > 1)
            yield return text[rangeStart..];
    }

    private static bool IsInRange(int index, IReadOnlyCollection<Range> ranges)
        => ranges.Any(r
            // Infinite ranges are implemented as zero-length
            => r.Start.Value == r.End.Value
               
            // Normal ranges are inclusive-exclusive
            || (r.Start.Value <= index && r.End.Value > index));
}