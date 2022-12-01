using Spectre.Console;
using System.Runtime.InteropServices;
using Spectre.Console.Cli;

namespace Collapse;

internal sealed class SimulateCommand : AsyncCommand<SimulateCommandSettings>
{
    private static readonly object _lock = new();

    public override async Task<int> ExecuteAsync(CommandContext context, SimulateCommandSettings settings)
    {
        var dotnetCommand = "dotnet";
        Action<IDictionary<string, string>> environmentSetup = null;

        // this is best effort workaround for ARM64...
        if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
        {
            dotnetCommand = "/usr/local/share/dotnet/x64/dotnet";
            var pathVariable = Environment.GetEnvironmentVariable("PATH");
            environmentSetup = env => env["PATH"] = $"{dotnetCommand}:{pathVariable}";
        }

        AnsiConsole.WriteLine();

        // build
        if (NeedsBuilding(settings))
        {
            var buildArgs = $"build {settings.Path} -c Release";
            await AnsiConsole.Status()
                .StartAsync("[yellow]Building...[/]", async ctx =>
                {
                    var (standardOutput, standardError) = await SimpleExec.Command.ReadAsync(dotnetCommand, args: buildArgs, configureEnvironment: environmentSetup);
                });

            AnsiConsole.MarkupLine(":check_mark: [green]Built successfully![/]");
        }
        else
        {
            AnsiConsole.MarkupLine(":check_mark: [green]Build skipped![/]");
        }

        // run
        var discoveryType = TryGetBestExecutionPath(settings.Path, out var path);
        var dotnetCommandArgs = discoveryType == DiscoveryType.Dll ? path : $"run --project {path} -c Release";
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
                        var (standardOutput, standardError) = await SimpleExec.Command.ReadAsync(dotnetCommand, args: dotnetCommandArgs, configureEnvironment: environmentSetup);

                        // only take the last line, because previous lines might contain any stdio output of the program itself
                        var result = SanitizeOutput(standardOutput);

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

        AnsiConsole.MarkupLine(":check_mark: [green]Finished running shots![/]");
        AnsiConsole.WriteLine();

        var chart = new BarChart()
            .Width(60)
            .Label("[green]Results:[/]");

        for (var i = 0; i < results.Count; i++)
        {
            chart.AddItem(results.ElementAt(i).Key.EscapeMarkup(), results.ElementAt(i).Value, PreferredColors[i]);
        }

        AnsiConsole.Write(chart);

        return 0;
    }

    private static readonly Color[] PreferredColors = new[] {
        Color.Yellow, Color.Green, Color.Aqua, Color.Blue
    };

    private static bool NeedsBuilding(SimulateCommandSettings settings)
    {
        if (settings.SkipBuild) return false;
        if (Path.HasExtension(settings.Path) && Path.GetExtension(settings.Path) == ".dll") return false;

        return true;
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
                return DiscoveryType.Dll;
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
                return DiscoveryType.Dll;

            }

            candidate = Path.Combine(path, "bin", "Debug", "net6.0", Path.GetFileNameWithoutExtension(csproj[0]) + ".dll");
            if (File.Exists(candidate))
            {
                discoveredPath = candidate;
                return DiscoveryType.Dll;
            }
        }

        discoveredPath = path;
        return DiscoveryType.Folder;
    }

    private static string SanitizeOutput(string standardOutput)
    {
        var rawResult = standardOutput.Trim().Split(Environment.NewLine).LastOrDefault();
        return rawResult.
            Replace("(", "|").
            Replace("[", "|").
            Replace(")", "⟩").
            Replace("]", "⟩").
            Replace(" ", string.Empty).
            Replace(",", string.Empty).
            Replace("Zero", "0").
            Replace("One", "1");
    }
}