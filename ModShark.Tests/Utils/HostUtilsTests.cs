using FluentAssertions;
using ModShark.Utils;

namespace ModShark.Tests.Utils;

public class HostUtilsTests
{
    private IEnumerable<string> FakeTargetHosts { get; } =
    [
        "example.com",
        "foo.com",
        "some.bar.org"
    ];
    
    [TestCase("")]
    [TestCase("    ")]
    [TestCase("example.com")]
    public void Matches_ShouldReturnFalse_ForEmptyTargets(string host)
    {
        var targets = Array.Empty<string>();

        var result = HostUtils.Matches(host, targets);

        result.Should().BeFalse();
    }
    
    [TestCase("example.com")]
    [TestCase("foo.com")]
    [TestCase("some.bar.org")]
    public void Matches_ShouldReturnTrue_ForExactMatches(string host)
    {
        var result = HostUtils.Matches(host, FakeTargetHosts);

        result.Should().BeTrue();
    }
    
    [TestCase(".example.com")]
    [TestCase("an.example.com")]
    [TestCase("the.foo.com")]
    [TestCase("at.some.bar.org")]
    public void Matches_ShouldReturnTrue_ForSubdomainMatches(string host)
    {
        var result = HostUtils.Matches(host, FakeTargetHosts);

        result.Should().BeTrue();
    }
    
    [TestCase("not-example.com")]
    [TestCase("foo.wrong")]
    [TestCase("at.a.different.bar.org")]
    public void Matches_ShouldReturnFalse_ForNonMatches(string host)
    {
        var result = HostUtils.Matches(host, FakeTargetHosts);

        result.Should().BeFalse();
    }
    
    [TestCase("")]
    [TestCase("    ")]
    [TestCase("example.com.")]
    [TestCase("example..com")]
    public void Matches_ShouldReturnFalse_ForInvalidHosts(string host)
    {
        var result = HostUtils.Matches(host, FakeTargetHosts);

        result.Should().BeFalse();
    }
}