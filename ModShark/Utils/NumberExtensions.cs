using System.Text;

namespace ModShark.Utils;

public static class NumberExtensions
{
    private static readonly char[] Base36 = [
        '0', '1', '2', '3',
        '4', '5', '6', '7',
        '8', '9', 'a', 'b',
        'c', 'd', 'e', 'f',
        
        'g', 'h', 'i', 'j',
        'k', 'l', 'm', 'n',
        'o', 'p', 'q', 'r',
        's', 't', 'u', 'v',
        
        'w', 'x', 'y', 'z'
    ];
    
    /// <summary>
    /// Encodes a number as a base-36 string.
    /// Conversion is identical to ECMAScript's <code>number.toString(36)</code>.
    /// </summary>
    public static string ToBase36String(this long number)
    {
        // Special-case for "0", since it's not supported by the loop.
        if (number == 0)
            return "0";
        
        var builder = new StringBuilder();

        // The loop only handles positive, so we need to address it here.
        // We write directly into the output builder to preserve order. 
        if (number < 0)
        {
            builder.Append('-');
            number = Math.Abs(number);
        }
        
        // This loop produces characters in reverse order, so we use a flip buffer to sort it out.
        var flipBuffer = new LinkedList<char>();
        while (number > 0)
        {
            var nextPart = number % 36;
            number /= 36;
            
            var nextChar = Base36[nextPart];
            flipBuffer.AddFirst(nextChar);
        }

        // Copy the flip buffer into the string builder, which restores the order.
        foreach (var chr in flipBuffer)
        {
            builder.Append(chr);
        }
        
        return builder.ToString();
    }
}