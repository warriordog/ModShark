using FluentAssertions;
using ModShark.Utils;
using SharkeyDB.Entities;

namespace ModShark.Tests.Utils;

public class InstanceExtensionsTests
{
    [Test]
    public void GetSoftwareString_ShouldReturnProperString_WhenBothFieldsArePresent()
    {
        var instance = new Instance
        {
            Id = "1",
            Host = "example.com",
            SuspensionState = "none",

            SoftwareName = "Sharkey",
            SoftwareVersion = "2024.6.0"
        };

        var result = instance.GetSoftwareString();

        result.Should().Be("Sharkey v2024.6.0");
    }

    [TestCase("Sharkey", null, "Sharkey v?")]
    [TestCase("Sharkey", "", "Sharkey v?")]
    [TestCase("Sharkey", " ", "Sharkey v ")]
    [TestCase(null, "2024.6.0", "? v2024.6.0")]
    [TestCase("", "2024.6.0", "? v2024.6.0")]
    [TestCase(" ", "2024.6.0", "  v2024.6.0")]
    public void GetSoftwareString_ShouldInsertPlaceholders_WhenAnyFieldIsMissing(string? name, string? version, string expected)
    {
        var instance = new Instance
        {
            Id = "1",
            Host = "example.com",
            SuspensionState = "none",

            SoftwareName = name,
            SoftwareVersion = version
        };

        var result = instance.GetSoftwareString();

        result.Should().Be(expected);
    }
}