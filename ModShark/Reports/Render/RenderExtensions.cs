using ModShark.Reports.Document;

namespace ModShark.Reports.Render;

public static class RenderExtensions
{
    public static TBuilder AppendFullFlaggedText<TBuilder>(this TBuilder builder, string text, IReadOnlyCollection<Range> ranges)
        where TBuilder : SegmentBase<TBuilder>
    {
        // Pick out the flagged ranges from the text
        var segments = SplitText(text, ranges).ToList();
        if (segments.Count < 1)
            return builder;
        
        var block = builder.BeginSpoiler();
        
        // Segments are interleaved (text,flag)
        var isFlag = false;
        foreach (var segment in segments)
        {
            if (segment.Length > 0)
            {
                if (isFlag)
                    AppendInlineFlag(block, segment);
                else
                    AppendInlineText(block, segment);
            }

            isFlag = !isFlag;
        }

        return block.End();
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

    public static TBuilder AppendMinimalFlaggedText<TBuilder>(this TBuilder builder, string text, IReadOnlyCollection<Range> ranges)
        where TBuilder : SegmentBase<TBuilder>
    {
        // Extract only the flagged segments
        var segments = ExtractSegments(text, ranges);
        
        // Append everything
        var first = true;
        foreach (var segment in segments)
        {
            if (!first)
                builder.AppendText(", ");
            first = false;

            var block = builder.BeginSpoiler();
            AppendInlineText(block, segment);
            block.End();
        }

        return builder;
    }

    private static IEnumerable<string> ExtractSegments(string text, IReadOnlyCollection<Range> ranges)
    {
        foreach (var r in ranges)
        {
            // Yikes
            // https://stackoverflow.com/questions/776430/why-is-the-iteration-variable-in-a-c-sharp-foreach-statement-read-only
            var range = r;
            
            // Ugly bounds checks :(
            if (range.Start.IsFromEnd && text.Length - range.Start.Value < 0)
                continue;
            if (!range.Start.IsFromEnd && range.Start.Value >= text.Length)
                continue;
            if (range.End.IsFromEnd && text.Length - range.End.Value < 0)
                continue;
            
            // Ugly bounds cap :(
            if (!range.End.IsFromEnd && range.End.Value >= text.Length)
                range = Range.StartAt(range.Start);
            
            yield return text[range];
        }
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
}