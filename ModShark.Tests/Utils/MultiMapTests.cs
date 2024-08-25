using FluentAssertions;
using ModShark.Utils;

namespace ModShark.Tests.Utils;

public class MultiMapTests
{
    private MultiMap<string, int> MapUnderTest { get; set; } = null!;
    
    [SetUp]
    public void Setup()
    {
        MapUnderTest = [];
        
        MapUnderTest.Add("a", 1);
        MapUnderTest.Add("a", 2);
        MapUnderTest.Add("a", 3);
        
        MapUnderTest.Add("b", 3);
        MapUnderTest.Add("b", 4);
        MapUnderTest.Add("b", 5);
    }
    
    [Test]
    public void Indexer1_ShouldReturnValue_WhenMapped()
    {
        MapUnderTest["a"].Should().BeEquivalentTo(new List<int> { 1, 2, 3 });
        MapUnderTest["b"].Should().BeEquivalentTo(new List<int> { 3, 4, 5 });
    }

    [Test]
    public void Indexer1_ShouldReturnNull_WhenNotMapped()
    {
        MapUnderTest["c"].Should().BeNull();
    }

    [Test]
    public void Indexer1_ShouldDeleteValue_WhenSetToNull()
    {
        MapUnderTest["a"] = null;

        MapUnderTest.Has("a").Should().BeFalse();
    }

    [Test]
    public void Indexer1_ShouldReplaceValues_WhenSet()
    {
        MapUnderTest["a"] = new HashSet<int> { 5 };
        
        MapUnderTest["a"].Should().BeEquivalentTo(new List<int> { 5 });
    }
    
    [Test]
    public void Indexer2_ShouldReturnTrue_WhenMapped()
    {
        MapUnderTest["a", 1].Should().BeTrue();
    }

    [Test]
    public void Indexer2_ShouldReturnFalse_WhenNotMapped()
    {
        MapUnderTest["a", 4].Should().BeFalse();
        MapUnderTest["c", 1].Should().BeFalse();
    }

    [Test]
    public void Indexer2_ShouldDeleteValue_WhenSetToFalse()
    {
        MapUnderTest["a", 1] = false;
        
        MapUnderTest["a"].Should().BeEquivalentTo(new List<int> { 2, 3 });
    }

    [Test]
    public void Indexer2_ShouldReplaceValues_WhenSetToTrue()
    {
        MapUnderTest["a", 4] = true;
        
        MapUnderTest["a"].Should().BeEquivalentTo(new List<int> { 1, 2, 3, 4 });
    }

    [Test]
    public void Add1_ShouldAddMapping()
    {
        MapUnderTest.Add(new KeyValuePair<string, int>("a", 4));

        MapUnderTest.Has("a", 4);
    }

    [Test]
    public void Add1_ShouldReturnTrue_WhenAdded()
    {
        var result = MapUnderTest.Add(new KeyValuePair<string, int>("a", 4));

        result.Should().BeTrue();
    }

    [Test]
    public void Add1_ShouldReturnFalse_WhenNotAdded()
    {
        var result = MapUnderTest.Add(new KeyValuePair<string, int>("a", 1));

        result.Should().BeFalse();
    }

    [Test]
    public void Add2_ShouldAddMapping()
    {
        MapUnderTest.Add("a", 4);

        MapUnderTest.Has("a", 4);
    }

    [Test]
    public void Add2_ShouldReturnTrue_WhenAdded()
    {
        var result = MapUnderTest.Add("a", 4);

        result.Should().BeTrue();
    }

    [Test]
    public void Add2_ShouldReturnFalse_WhenNotAdded()
    {
        var result = MapUnderTest.Add("a", 1);

        result.Should().BeFalse();
    }

    [Test]
    public void AddRange_ShouldAddAllValues()
    {
        MapUnderTest.AddRange("a", new List<int> { 3, 4, 5 });

        var expected = new HashSet<int> { 1, 2, 3, 4, 5 };
        MapUnderTest.Get("a").Should().BeEquivalentTo(expected);
    }

    [Test]
    public void AddRange_ShouldReturnNumberAdded()
    {
        var expected = MapUnderTest.AddRange("a", new List<int> { 3, 4, 5 });

        expected.Should().Be(2);
    }

    [Test]
    public void Set_ShouldReplaceValues()
    {
        MapUnderTest.Set("a", new List<int> { 3, 4, 5 });

        var expected = new HashSet<int> { 3, 4, 5 };
        MapUnderTest.Get("a").Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Contains_ShouldReturnTrue_WhenPresent()
    {
        MapUnderTest.Contains(new KeyValuePair<string, int>("a", 1)).Should().BeTrue();
    }

    [Test]
    public void Contains_ShouldReturnFalse_WhenNotPresent()
    {
        MapUnderTest.Contains(new KeyValuePair<string, int>("a", 4)).Should().BeFalse();
    }

    [Test]
    public void Has1_ShouldReturnTrue_WhenPresent()
    {
        MapUnderTest.Has("a").Should().BeTrue();
    }

    [Test]
    public void Has1_ShouldReturnFalse_WhenNotPresent()
    {
        MapUnderTest.Has("c").Should().BeFalse();
    }

    [Test]
    public void Has2_ShouldReturnTrue_WhenMapped()
    {
        MapUnderTest.Has("a", 1).Should().BeTrue();
    }

    [Test]
    public void Has2_ShouldReturnFalse_WhenNotMapped()
    {
        MapUnderTest.Has("a", 4).Should().BeFalse();
    }

    [Test]
    public void Remove_ShouldRemoveMapping()
    {
        MapUnderTest.Remove(new KeyValuePair<string, int>("a", 2));

        MapUnderTest.Has("a", 2).Should().BeFalse();
    }

    [Test]
    public void Remove_ShouldReturnTrue_WhenRemoved()
    {
        var result = MapUnderTest.Remove(new KeyValuePair<string, int>("a", 2));

        result.Should().BeTrue();
    }

    [Test]
    public void Remove_ShouldReturnFalse_WhenNotRemoved()
    {
        var result = MapUnderTest.Remove(new KeyValuePair<string, int>("a", 4));

        result.Should().BeFalse();
    }

    [Test]
    public void Delete_ShouldRemoveMapping()
    {
        MapUnderTest.Delete("a", 2);

        MapUnderTest.Has("a", 2).Should().BeFalse();
    }

    [Test]
    public void Delete_ShouldReturnTrue_WhenRemoved()
    {
        var result = MapUnderTest.Delete("a", 2);

        result.Should().BeTrue();
    }

    [Test]
    public void Delete_ShouldReturnFalse_WhenNotRemoved()
    {
        var result = MapUnderTest.Delete("a", 4);

        result.Should().BeFalse();
    }

    [Test]
    public void Get_ShouldReturnCollection_WhenMapped()
    {
        var actual = MapUnderTest.Get("a");

        var expected = new HashSet<int> { 1, 2, 3 };
        actual.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Get_ShouldReturnNull_WhenNotMapped()
    {
        var actual = MapUnderTest.Get("c");

        actual.Should().BeNull();
    }

    [Test]
    public void TryGet_ShouldReturnTrue_WhenMapped()
    {
        var result = MapUnderTest.TryGet("a", out _);

        result.Should().BeTrue();
    }

    [Test]
    public void TryGet_ShouldReturnFalse_WhenNotMapped()
    {
        var result = MapUnderTest.TryGet("c", out _);

        result.Should().BeFalse();
    }

    [Test]
    public void TryGet_ShouldOutSet_WhenMapped()
    {
        MapUnderTest.TryGet("a", out var set);

        var expected = new HashSet<int> { 1, 2, 3 };
        set.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void TryGet_ShouldOutNull_WhenNotMapped()
    {
        MapUnderTest.TryGet("c", out var set);

        set.Should().BeNull();
    }

    [Test]
    public void Count_ShouldReturnNumberOfValues()
    {
        MapUnderTest.Count.Should().Be(6);
    }

    [Test]
    public void Size0_ShouldReturnNumberOfKeys()
    {
        MapUnderTest.Size().Should().Be(2);
    }

    [Test]
    public void Size1_ShouldReturnNumberOfValuesForKey()
    {
        MapUnderTest.Size("a").Should().Be(3);
    }
    
    [Test]
    public void TotalSize_ShouldReturnNumberOfValues()
    {
        MapUnderTest.TotalSize().Should().Be(6);
    }

    [Test]
    public void Keys_ShouldReturnKeys()
    {
        var expected = new HashSet<string> { "a", "b" };

        MapUnderTest.Keys.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Values_ShouldReturnValues()
    {
        var expected = new HashSet<int> { 1, 2, 3, 4, 5 };

        MapUnderTest.Values.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Entries_ShouldReturnAllPairs()
    {
        var expected = new List<KeyValuePair<string, int>>
        {
            new("a", 1),
            new("a", 2),
            new("a", 3),
            new("b", 3),
            new("b", 4),
            new("b", 5),
        };

        MapUnderTest.Entries.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Mappings_ShouldReturnAllMappings()
    {
        var expected = new Dictionary<string, HashSet<int>>
        {
            ["a"] = [1, 2, 3],
            ["b"] = [3, 4, 5]
        };

        MapUnderTest.Mappings.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void Clear_ShouldRemoveAllEntries()
    {
        MapUnderTest.Clear();

        MapUnderTest.Should().BeEmpty();
    }

    [Test]
    public void CopyTo_ShouldThrow_WhenIndexIsNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            MapUnderTest.CopyTo([], -1);
        });
    }

    [Test]
    public void CopyTo_ShouldThrow_WhenArrayIsTooSmall()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            MapUnderTest.CopyTo([], 0);
        });
    }

    [Test]
    public void CopyTo_ShouldCopyAllValues()
    {
        var expected = new[]
        {
            default,
            new KeyValuePair<string, int>("a", 1),
            new KeyValuePair<string, int>("a", 2),
            new KeyValuePair<string, int>("a", 3),
            new KeyValuePair<string, int>("b", 3),
            new KeyValuePair<string, int>("b", 4),
            new KeyValuePair<string, int>("b", 5),
        };
        
        var array = new KeyValuePair<string, int>[7];
        MapUnderTest.CopyTo(array, 1);

        array.Should().BeEquivalentTo(expected);
    }
}