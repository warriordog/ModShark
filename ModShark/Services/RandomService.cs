using System.Security.Cryptography;

namespace ModShark.Services;

public interface IRandomService
{
    /// <inheritdoc cref="RandomNumberGenerator"/>
    byte[] GetBytes(int length);

    /// <inheritdoc cref="RandomNumberGenerator"/>
    string GetString(char[] choices, int length);

    /// <inheritdoc cref="RandomNumberGenerator"/>
    string GetHexString(int length, bool lowerCase);
}

public class RandomService : IRandomService
{
    public byte[] GetBytes(int length)
        => RandomNumberGenerator.GetBytes(length);

    public string GetString(char[] choices, int length)
        => RandomNumberGenerator.GetString(choices, length);

    public string GetHexString(int length, bool lowerCase)
        => RandomNumberGenerator.GetHexString(length, lowerCase);
}