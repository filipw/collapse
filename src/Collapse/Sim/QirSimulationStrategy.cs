namespace Collapse;

public class QirSimulationStrategy : ISimulationStrategy
{
    private readonly SimulateCommandSettings settings;

    public QirSimulationStrategy(SimulateCommandSettings settings)
    {
        this.settings = settings;
    }

    public CommandLineInfo GetBuildCommandLineInfo(string path)
    {
        if (!NeedsBuilding(path)) return CommandLineInfo.None;

        return new CommandLineInfo
        {
            Title = "Building QIR",
            Name = "dotnet",
            Args = $"build {path} /p:QirGeneration=true /p:CSharpGeneration=false"
        };
    }

    public CommandLineInfo GetExecuteCommandLineInfo(string path)
    {
        var discoveryType = TryGetBestExecutionPath(path, out var discoveredPath);
        if (discoveryType == DiscoveryType.NotFound)
        {
            throw new Exception("No valid QIR executable found!");
        }

        return new CommandLineInfo
        {
            Name = !string.IsNullOrWhiteSpace(settings.QirRunner) ? settings.QirRunner : "qir-runner",
            Args = discoveredPath
        };
    }

    private bool NeedsBuilding(string path)
    {
        if (settings.SkipBuild)
        {
            return false;
        }

        if (Path.HasExtension(path) && Path.GetExtension(path) == ".ll") return false;
        return true;
    }

    private static DiscoveryType TryGetBestExecutionPath(string path, out string discoveredPath)
    {
        discoveredPath = null;
        if (path == null)
        {
            return DiscoveryType.NotFound;
        }

        if (Path.HasExtension(path))
        {
            var extension = Path.GetExtension(path);
            if (extension.ToLowerInvariant() is ".ll")
            {
                discoveredPath = path;
                return DiscoveryType.Executable;
            }
        }

        var csproj = Directory.GetFiles(path, "*.csproj");
        if (csproj.Any())
        {
            // search in release folder
            var candidate = Path.Combine(path, "qir", Path.GetFileNameWithoutExtension(csproj[0]) + ".ll");
            if (File.Exists(candidate))
            {
                discoveredPath = candidate;
                return DiscoveryType.Executable;
            }
        }

        return DiscoveryType.NotFound;
    }
}
