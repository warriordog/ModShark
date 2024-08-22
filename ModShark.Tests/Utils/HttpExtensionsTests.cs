using System.Net.Http.Headers;
using System.Reflection;
using FluentAssertions;
using ModShark.Utils;

namespace ModShark.Tests.Utils;

public class HttpExtensionsTests
{
    private HttpResponseHeaders TestHeaders { get; set; } = null!;

    [SetUp]
    public void Setup()
    {
        TestHeaders = (HttpResponseHeaders)Activator.CreateInstance(
            typeof(HttpResponseHeaders),
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, 
            null,
            [ false ],
            null
        )!;
        
        TestHeaders.Add("StringSingle", "StringSingle value");
        TestHeaders.Add("StringDouble", "StringDouble value 1");
        TestHeaders.Add("StringDouble", "StringDouble value 2");
        TestHeaders.Add("IntSingle", "1");
    }
    
    [Test]
    public void GetShould_ReturnNull_WhenHeaderIsMissing()
    {
        TestHeaders.Get("header").Should().BeNull();
    }
    
    [Test]
    public void GetShould_Throw_WhenHeaderIsDuplicate()
    {
        TestHeaders.Add("header", "value 1");
        TestHeaders.Add("header", "value 2");
        
        Assert.Throws<InvalidOperationException>(() => 
            TestHeaders.Get("header")
        );
    }
    
    [Test]
    public void GetShould_ReturnValue_WhenHeaderExists()
    {
        TestHeaders.Add("header", "value");

        var result = TestHeaders.Get("header");

        result.Should().Be("value");
    }
    
    [Test]
    public void GetNumericShould_ReturnNull_WhenHeaderIsMissing()
    {
        TestHeaders.GetNumeric<int>("header").Should().BeNull();
    }
    
    [Test]
    public void GetNumericShould_Throw_WhenHeaderIsDuplicate()
    {
        TestHeaders.Add("header", "1");
        TestHeaders.Add("header", "2");
        
        Assert.Throws<InvalidOperationException>(() => 
            TestHeaders.GetNumeric<int>("header")
        );
    }
    
    [Test]
    public void GetNumericShould_Throw_WhenHeaderHasWrongType()
    {
        TestHeaders.Add("header", "wrong");
        
        Assert.Throws<FormatException>(() => 
            TestHeaders.GetNumeric<int>("header")
        );
    }
    
    [TestCase("0", 0)]
    [TestCase("10", 10)]
    [TestCase("1.0", 1.0)]
    [TestCase("-1.0", -1.0)]
    public void GetNumericShould_ParseValue_WhenHeaderExists(string input, double expected)
    {
        TestHeaders.Add("header", input);

        var actual = TestHeaders.GetNumeric<double>("header");

        actual.Should().Be(expected);
    }
}