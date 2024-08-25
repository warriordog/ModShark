using FluentAssertions;
using ModShark.Utils;

namespace ModShark.Tests.Utils;

public class CollectionExtensionsTests
{
    [Test]
    public void ForEach_ShouldIterateAllValues()
    {
        var expected = new List<int> { 1, 2, 3 };

        var actual = new List<int>();
        (expected as IEnumerable<int>).ForEach((value, _) => actual.Add(value));

        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void ForEach_ShouldCountIndex()
    {
        IEnumerable<int> input = new List<int> { 1, 2, 3 };

        var actual = new List<int>();
        input.ForEach((_, i) => actual.Add(i));

        var expected = new List<int> { 0, 1, 2 };
        actual.Should().BeEquivalentTo(expected);
    }
}