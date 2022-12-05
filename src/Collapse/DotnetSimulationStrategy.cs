namespace Collapse;

public class DotnetSimulationStrategy : ISimulationStrategy
{
    public CommandLineInfo GetBuildCommandLineInfo(string path)
    {
        if (!NeedsBuilding(path)) return CommandLineInfo.None;

        return new CommandLineInfo
        {
            Name = "dotnet",
            Args = $"build {path} -c Release /p:QirGeneration=false /p:CSharpGeneration=true"
        };
    }

    public CommandLineInfo GetSimulateCommandLineInfo(SimulateCommandSettings settings)
    {
        var discoveryType = TryGetBestExecutionPath(settings.Path, out var path);
        var dotnetCommandArgs = discoveryType == DiscoveryType.Executable ? path : $"run --project {path} -c Release";

        return new CommandLineInfo
        {
            Name = "dotnet",
            Args = dotnetCommandArgs
        };
    }

    private static DiscoveryType TryGetBestExecutionPath(string path, out string discoveredPath)
    {
        if (path == null)
        {
            discoveredPath = Directory.GetCurrentDirectory();
            return DiscoveryType.CurrentDirectory;
        }

        if (Path.HasExtension(path))
        {
            var extension = Path.GetExtension(path);
            if (extension is ".dll")
            {
                discoveredPath = path;
                return DiscoveryType.Executable;
            }
        }

        var csproj = Directory.GetFiles(path, "*.csproj");
        if (csproj.Any())
        {
            // search in release folder
            var candidate = Path.Combine(path, "bin", "Release", "net6.0", Path.GetFileNameWithoutExtension(csproj[0]) + ".dll");
            if (File.Exists(candidate))
            {
                discoveredPath = candidate;
                return DiscoveryType.Executable;

            }

            candidate = Path.Combine(path, "bin", "Debug", "net6.0", Path.GetFileNameWithoutExtension(csproj[0]) + ".dll");
            if (File.Exists(candidate))
            {
                discoveredPath = candidate;
                return DiscoveryType.Executable;
            }
        }

        discoveredPath = path;
        return DiscoveryType.Folder;
    }

    private static bool NeedsBuilding(string path)
    {
        if (Path.HasExtension(path) && Path.GetExtension(path) == ".dll") return false;
        return true;
    }
}