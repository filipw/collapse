using Xunit;

namespace Collapse.Tests;

public class QirSimulationStrategyTests 
{
    [Fact]
    public void BuildCommandSkipsBuildWhenRequested()
    {
        var settings = new SimulateCommandSettings { SkipBuild = true };
        var strategy = new QirSimulationStrategy(settings);
        var result = strategy.GetBuildCommandLineInfo(".");

        Assert.Equal(CommandLineInfo.None, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("/foo/qir-runner")]
    public void ExecuteCommandReturnsExpectedArgumentsForAnLlFile(string qirRunnerPath)
    {
        var settings = new SimulateCommandSettings { QirRunner = qirRunnerPath};
        var strategy = new QirSimulationStrategy(settings);
        //var result = strategy.GetExecuteCommandLineInfo("../../../../../test-assets/H/");
        var result = strategy.GetExecuteCommandLineInfo("foo.ll");

        Assert.Equal("foo.ll", result.Args);
        Assert.Equal(string.IsNullOrWhiteSpace(qirRunnerPath) ? "qir-runner" : "/foo/qir-runner", result.Name);
    }
}