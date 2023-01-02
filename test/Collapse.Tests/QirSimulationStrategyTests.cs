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
}