using Spectre.Console;

namespace Collapse;

public class QirSimulationStrategy : ISimulationStrategy
{
    private static readonly object _lock = new();

    public async Task<Dictionary<string, int>> Simulate(SimulateCommandSettings settings)
    {
        var discoveryType = TryGetBestExecutionPath(settings.Path, out var path);
        if (discoveryType == DiscoveryType.NotFound)
        {
            throw new Exception("No valid QIR executable found!");
        }

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
                        var (standardOutput, standardError) = await SimpleExec.Command.ReadAsync("qir-runner", args: path);

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
