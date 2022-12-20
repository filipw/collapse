using Xunit;

namespace Collapse.Tests;

public class AzureSyncExecutionStrategyTests
{
    [Fact]
    public void ThrowsWithoutTargetId()
    {
        var settings = new AzureCommandSettings();
        var strategy = new AzureSyncExecutionStrategy(settings);
        Assert.Throws<Exception>(() => strategy.GetExecuteCommandLineInfo("/"));
    }

    [Fact]
    public void BuildsCorrectCommandFromSettings() 
    {
        var settings = new AzureCommandSettings
        {
            Path = "/foo",
            TargetId = "ionq.qpu",
            Shots = 1024
        };
        var strategy = new AzureSyncExecutionStrategy(settings);
        var result = strategy.GetExecuteCommandLineInfo("/foo");

        Assert.Equal("az", result.Name);
        Assert.Equal($"quantum execute --project {settings.Path} --target-id {settings.TargetId} --shots {settings.Shots} --output json", result.Args);
    }
}
