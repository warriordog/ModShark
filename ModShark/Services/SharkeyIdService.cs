using System.Text;
using ModShark.Utils;

namespace ModShark.Services;

/// <summary>
/// Generates Misskey-compatible ID values in any of the many supported formats.
/// These ID strings are absolutely FUCKED because they contain *parseable* embedded timestamps.
/// </summary>
public interface ISharkeyIdService
{
    /// <summary>
    /// Generate an ID for the current time with the instance's configured format.
    /// </summary>
    string GenerateId();
    
    /// <summary>
    /// Generate an ID with the instance's configured format
    /// </summary>
    /// <param name="time">Timestamp of the ID</param>
    string GenerateId(DateTime time);

    /// <summary>
    /// Generate an ID with a specified format
    /// </summary>
    /// <param name="time">Timestamp of the ID</param>
    /// <param name="format">Format to generate</param>
    string GenerateId(DateTime time, IdFormat format);
}

public enum IdFormat
{
    Aid,
    AidX,
    MeId,
    MeIdG,
    ULId,
    ObjectId
}

public class SharkeyIdService(SharkeyConfig config, IRandomService randomService, ITimeService timeService) : ISharkeyIdService
{
    private const long Time2000 = 946684800000;
    private static readonly long UlIdTimeMax = (long)Math.Pow(2, 48) - 1;
    
    private long NextCounter
    {
        get
        {
            _counter++;
            return _counter;
        }
    }
    private long _counter = BitConverter.ToUInt16(randomService.GetBytes(2));
    
    private string NodeId { get; } = randomService.GetString(NodeIdAlphabet, 4);

    public string GenerateId()
        => GenerateId(timeService.UtcNow);
    
    public string GenerateId(DateTime time)
        => GenerateId(time, config.IdFormat);
    
    public string GenerateId(DateTime time, IdFormat format)
    {
        if (time.Kind != DateTimeKind.Utc)
            throw new ArgumentOutOfRangeException(nameof(time), time, "DateTime inputs must be in UTC");
        
        var timeValue = ((DateTimeOffset)time).ToUnixTimeMilliseconds();
        return format switch
        {
            IdFormat.Aid => GenerateAid(timeValue),
            IdFormat.AidX => GenerateAidX(timeValue),
            IdFormat.MeId => GenerateMeId(timeValue),
            IdFormat.MeIdG => GenerateMeIdG(timeValue),
            IdFormat.ULId => GenerateUlId(timeValue),
            IdFormat.ObjectId => GenerateObjectId(timeValue),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown ID format")
        };
    }

    private string GenerateAid(long time)
    {
        time = Math.Max(time - Time2000, 0);
        var timeComponent = time
            .ToBase36String()
            .PadLeft(8, '0');

        var noiseComponent = NextCounter
            .ToBase36String()
            .PadLeft(2, '0')
            [^2..];

        return timeComponent + noiseComponent;
    }

    private string GenerateAidX(long time)
    {
        time = Math.Max(time - Time2000, 0);
        var timeComponent = time
            .ToBase36String()
            .PadLeft(8, '0')
            [^8..];


        var noiseComponent = NextCounter
            .ToBase36String()
            .PadLeft(4, '0')
            [^4..];
        
        return timeComponent + NodeId + noiseComponent;
    }

    private string GenerateMeId(long time)
    {
        var timeComponent = GenerateMeIdTime(time);
        var randomComponent = randomService.GetHexString(12, true);

        return timeComponent + randomComponent;
    }

    private string GenerateMeIdTime(long time)
    {
        time = Math.Max(time, 0);
        if (time == 0)
            return "0";

        time += 0x800000000000;
        return Convert
            .ToString(time, 16)
            .PadLeft(12, '0');
    }

    private string GenerateMeIdG(long time)
    {
        var timeComponent = GenerateMeIdGTime(time);
        var randomComponent = randomService.GetHexString(12, true);

        return "g" + timeComponent + randomComponent;
    }

    private string GenerateMeIdGTime(long time)
    {
        time = Math.Max(time, 0);
        if (time == 0)
            return "0";
        
        return Convert
            .ToString(time, 16)
            .PadLeft(11, '0');
    }

    private string GenerateUlId(long time)
    {
        if (time > UlIdTimeMax)
            throw new ArgumentOutOfRangeException(nameof(time), time, $"Cannot generate a value larger than {UlIdTimeMax}");

        var encodingLength = (long)UlIdAlphabet.Length;
        var flipBuffer = new LinkedList<char>();
        for (var len = 10; len > 0; len--) {
            var mod = time % encodingLength;
            time = (time - mod) / encodingLength;
            
            var chr = UlIdAlphabet[mod];
            flipBuffer.AddFirst(chr);
        }
        
        // Reverse and build the string
        var timeComponentBuilder = new StringBuilder();
        foreach (var chr in flipBuffer)
        {
            timeComponentBuilder.Append(chr);
        }
        var timeComponent = timeComponentBuilder.ToString();

        var randomComponent = randomService.GetString(UlIdAlphabet, 16);

        return timeComponent + randomComponent;
    }

    private string GenerateObjectId(long time)
    {
        var timeComponent = GenerateObjectIdTime(time);
        var randomComponent = randomService.GetHexString(16, true);

        return timeComponent + randomComponent;
    }

    private string GenerateObjectIdTime(long time)
    {
        time = Math.Max(time, 0);
        if (time == 0)
            return "0";
        
        time /= 1000;
        return Convert
            .ToString(time, 16)
            .PadLeft(8, '0');
    }

    private static readonly char[] NodeIdAlphabet =
    [
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

    private static readonly char[] UlIdAlphabet =
    [
        '0', '1', '2', '3',
        '4', '5', '6', '7',
        '8', '9', 'A', 'B',
        'C', 'D', 'E', 'F',
        
        'G', 'H',      'J',
        'K',      'M', 'N',
             'P', 'Q', 'R',
        'S', 'T',      'V',
        
        'W', 'X', 'Y', 'Z'
    ];
}