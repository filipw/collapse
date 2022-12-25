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

    [Fact]
    public void BuildCommandReturnsExpectedArguments()
    {
        var strategy = new DotnetSimulationStrategy();
        var result = strategy.GetBuildCommandLineInfo(".");

        Assert.Equal("build . -c Release /p:QirGeneration=false /p:CSharpGeneration=true", result.Args);
        Assert.Equal("dotnet", result.Name);
        Assert.Equal("Building C#", result.Title);
    }

    [Fact]
    public void BuildCommandSkipsBuildForDllPaths()
    {
        var strategy = new DotnetSimulationStrategy();
        var result = strategy.GetBuildCommandLineInfo("foo.dll");

        Assert.Equal(CommandLineInfo.None, result);
    }
}