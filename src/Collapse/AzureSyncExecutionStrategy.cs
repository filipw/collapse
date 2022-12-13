namespace Collapse;

public class AzureSyncExecutionStrategy : IExecutionStrategy
{
    private readonly AzureCommandSettings settings;

    public AzureSyncExecutionStrategy(AzureCommandSettings settings)
    {
        this.settings = settings;
    }

    public CommandLineInfo GetExecuteCommandLineInfo(string path)
    {
        if (string.IsNullOrWhiteSpace(settings.TargetId)) throw new Exception("Target is mandatory!");

        var command = $"quantum execute --project {settings.Path} --target-id {settings.TargetId} --shots {settings.Shots} --output json";

        if (settings.SkipBuild)
        {
            command = $"{command} --no-build";
        }
        
        return new CommandLineInfo
        {
            Name = "az",
            Args = command
        };
    }
}