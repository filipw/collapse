using Xunit;

namespace Collapse.Tests;

public class DotnetSimulationStrategyTests 
{
    [Fact]
    public void ExecuteCommandReturnsExpectedArgumentsForAFolder()
    {
        var strategy = new DotnetSimulationStrategy();
        var result = strategy.GetExecuteCommandLineInfo(".");

        Assert.Equal("run --project . -c Release", result.Args);
        Assert.Equal("dotnet", result.Name);
    }
}
