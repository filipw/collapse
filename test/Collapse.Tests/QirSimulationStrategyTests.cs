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

    [Fact]
    public void BuildCommandSkipsBuildForAnLllFile()
    {
        var settings = new SimulateCommandSettings();
        var strategy = new QirSimulationStrategy(settings);
        var result = strategy.GetBuildCommandLineInfo("foo.ll");

        Assert.Equal(CommandLineInfo.None, result);
    }

    [Fact]
    public void ExecuteCommandThrowsOnUnknownPath()
    {
        var settings = new SimulateCommandSettings();
        var strategy = new QirSimulationStrategy(settings);

        Assert.Throws<Exception>(() => strategy.GetExecuteCommandLineInfo("."));
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