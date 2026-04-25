using GodotMcp.Core.Models;

namespace GodotMcp.Tests.CoreTests;

public class DocumentationMcpValidationTests
{
    [Theory]
    [InlineData("manifest")]
    [InlineData("MARKDOWN")]
    [InlineData("Both")]
    public void NormalizeSource_AcceptsValidValues(string input)
    {
        var s = DocumentationMcpValidation.NormalizeSource(input);
        Assert.Equal(input.Trim().ToLowerInvariant(), s);
    }

    [Fact]
    public void NormalizeSource_DefaultsToBoth()
    {
        Assert.Equal(DocumentationMcpValidation.SourceBoth, DocumentationMcpValidation.NormalizeSource(null));
        Assert.Equal(DocumentationMcpValidation.SourceBoth, DocumentationMcpValidation.NormalizeSource("   "));
    }

    [Fact]
    public void NormalizeSource_RejectsInvalid()
    {
        Assert.Throws<ArgumentException>(() => DocumentationMcpValidation.NormalizeSource("all"));
    }

    [Fact]
    public void ValidateEngineQuery_RejectsEmpty()
    {
        Assert.Throws<ArgumentException>(() => DocumentationMcpValidation.ValidateEngineQuery(""));
        Assert.Throws<ArgumentException>(() => DocumentationMcpValidation.ValidateEngineQuery("   "));
    }

    [Fact]
    public void NormalizeEngineVersion_RejectsUrlScheme()
    {
        Assert.Throws<ArgumentException>(() => DocumentationMcpValidation.NormalizeEngineVersion("https://x"));
    }

    [Fact]
    public void NormalizeEngineVersion_RejectsTooLong()
    {
        Assert.Throws<ArgumentException>(() => DocumentationMcpValidation.NormalizeEngineVersion(new string('a', 65)));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(50, 40)]
    [InlineData(12, 12)]
    public void ClampEngineMaxResults_ClampsToRange(int input, int expected)
    {
        Assert.Equal(expected, DocumentationMcpValidation.ClampEngineMaxResults(input));
    }

    [Fact]
    public void QueryGodotEngineDocumentationRequest_ClampsMaxResults()
    {
        var r = new QueryGodotEngineDocumentationRequest(Root, "Node", maxResults: 999);
        Assert.Equal(40, r.MaxResults);
    }

    [Fact]
    public void QueryGodotEngineDocumentationRequest_RequiresNonEmptyQuery()
    {
        Assert.Throws<ArgumentException>(() => new QueryGodotEngineDocumentationRequest(Root, ""));
    }
}
