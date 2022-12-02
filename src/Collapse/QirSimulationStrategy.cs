namespace Collapse;

public class QirSimulationStrategy : ISimulationStrategy
{
    public CommandLineInfo GetSimulateCommandLineInfo(SimulateCommandSettings settings)
    {
        var discoveryType = TryGetBestExecutionPath(settings.Path, out var path);
        if (discoveryType == DiscoveryType.NotFound)
        {
            throw new Exception("No valid QIR executable found!");
        }

        return new CommandLineInfo
        {
            Name = !string.IsNullOrWhiteSpace(settings.QirRunner) ? settings.QirRunner : "qir-runner",
            Args = path
        };
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
