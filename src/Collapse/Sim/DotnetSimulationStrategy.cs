namespace Collapse;

public class DotnetSimulationStrategy : ISimulationStrategy
{
    private readonly SimulateCommandSettings settings;

    public DotnetSimulationStrategy(SimulateCommandSettings settings)
    {
        this.settings = settings;
    }

    public CommandLineInfo GetBuildCommandLineInfo(string path)
    {
        if (!NeedsBuilding(path)) return CommandLineInfo.None;

        return new CommandLineInfo
        {
            Title = "Building C#",
            Name = "dotnet",
            Args = $"build {path} -c Release /p:QirGeneration=false /p:CSharpGeneration=true"
        };
    }

    public CommandLineInfo GetExecuteCommandLineInfo(string path)
    {
        var discoveryType = TryGetBestExecutionPath(path, out var discoveredPath);
        var dotnetCommandArgs = discoveryType == DiscoveryType.Executable ? discoveredPath : $"run --project {discoveredPath} -c Release";

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

    private bool NeedsBuilding(string path)
    {
        if (settings.SkipBuild)
        {
            return false;
        }

        if (Path.HasExtension(path) && Path.GetExtension(path) == ".dll") return false;
        return true;
    }
}