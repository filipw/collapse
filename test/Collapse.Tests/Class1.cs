using Xunit;

namespace Collapse.Tests;

public class OutputParserTests
{
    [Theory]
    [InlineData("Zero", "|0⟩")]
    [InlineData("One", "|1⟩")]
    [InlineData("(Zero, One)", "|01⟩")]
    [InlineData("[Zero, One]", "|01⟩")]
    public void BasicOutputTests(string input, string expectedOutput)
    {
        var result = OutputParser.SanitizeOutput(input);
        Assert.Equal(expectedOutput, result);
    }
}
