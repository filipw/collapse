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
}
