using Xunit;

namespace Collapse.Tests;

public class OutputParserTests
{
    [Theory]
    [InlineData("Zero", "|0⟩")]
    [InlineData("One", "|1⟩")]
    [InlineData("(Zero, One)", "|01⟩")]
    [InlineData("[Zero, One]", "|01⟩")]
    public void BasicQSharpOutput(string input, string expectedOutput)
    {
        var result = OutputParser.SanitizeOutput(input);
        Assert.Equal(expectedOutput, result);
    }

    [Fact]
    public void BasicAzureSimulatorOutput()
    {
        var simJsonPath = Path.Combine("..", "..", "..", "..", "..", "test-assets", "azure-outputs", "simulator.json");
        var contents = File.ReadAllText(simJsonPath);

        var result = OutputParser.ParseResults(contents);
        Assert.Equal(16, result.Count);
        Assert.Equal("|0000⟩", result.Keys.ElementAt(0));
        Assert.Equal("|1000⟩", result.Keys.ElementAt(1));
        Assert.Equal("|0100⟩", result.Keys.ElementAt(2));
        Assert.Equal("|1100⟩", result.Keys.ElementAt(3));
        Assert.Equal("|0010⟩", result.Keys.ElementAt(4));
        Assert.Equal("|1010⟩", result.Keys.ElementAt(5));
        Assert.Equal("|0110⟩", result.Keys.ElementAt(6));
        Assert.Equal("|1110⟩", result.Keys.ElementAt(7));
        Assert.Equal("|0001⟩", result.Keys.ElementAt(8));
        Assert.Equal("|1001⟩", result.Keys.ElementAt(9));
        Assert.Equal("|0101⟩", result.Keys.ElementAt(10));
        Assert.Equal("|1101⟩", result.Keys.ElementAt(11));
        Assert.Equal("|0011⟩", result.Keys.ElementAt(12));
        Assert.Equal("|1011⟩", result.Keys.ElementAt(13));
        Assert.Equal("|0111⟩", result.Keys.ElementAt(14));
        Assert.Equal("|1111⟩", result.Keys.ElementAt(15));
    }

    [Theory]
    [InlineData("// 453 foo")]
    [InlineData("{ // 453 foo }")]
    [InlineData("{ \"brr\" }")]
    public void BadSimulatorOutput(string data)
    {
        var result = OutputParser.ParseResults(data);
        Assert.Empty(result);
    }
}
