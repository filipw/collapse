using Spectre.Console;

namespace Collapse;

public class DotnetSimulationStrategy : ISimulationStrategy
{
    private static readonly object _lock = new();

    public async Task<Dictionary<string, int>> Simulate(SimulateCommandSettings settings)
    {
        var discoveryType = TryGetBestExecutionPath(settings.Path, out var path);
        var dotnetCommandArgs = discoveryType == DiscoveryType.Executable ? path : $"run --project {path} -c Release";
        var stepSize = Math.Round(100.0 / settings.Shots, 2);
        var results = new Dictionary<string, int>();

        await AnsiConsole.Progress()
            .Columns(new ProgressColumn[]
            {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn()
            })
            .StartAsync(async ctx =>
            {
                var shotTask = ctx.AddTask("[yellow]Running shots[/]");

                var chunks = Enumerable.Range(0, settings.Shots).Chunk(5);
                foreach (var chunk in chunks)
                {
                    await Task.WhenAll(chunk.Select(i => Task.Run(async () =>
                    {
                        var (standardOutput, standardError) = await SimpleExec.Command.ReadAsync("dotnet", args: dotnetCommandArgs);

                        // only take the last line, because previous lines might contain any stdio output of the program itself
                        var result = standardOutput.SanitizeOutput();

                        lock (_lock)
                        {
                            if (result != null)
                            {
                                if (results.ContainsKey(result))
                                {
                                    results[result] += 1;
                                }
                                else
                                {
                                    results[result] = 1;
                                }
                            }
                        }
                    })));

                    shotTask.Increment(stepSize * chunk.Length);
                }

                shotTask.Value = 100;
            });

        return results;
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
}