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

        // 1. choose strategy
        ISimulationStrategy simulation = settings.Qir ? new QirSimulationStrategy() : new DotnetSimulationStrategy();

        // 2. build
        if (!settings.SkipBuild)
        {
            var buildCommandLineInfo = simulation.GetBuildCommandLineInfo(settings.Path);
            if (buildCommandLineInfo != CommandLineInfo.None)
            {
                await AnsiConsole.Status()
                    .StartAsync("[yellow]Building...[/]", async ctx =>
                    {
                        var (standardOutput, standardError) = await SimpleExec.Command.ReadAsync(buildCommandLineInfo.Name, args: buildCommandLineInfo.Args, configureEnvironment: environmentSetup);
                    });

                AnsiConsole.MarkupLine(":check_mark: [green]Built successfully![/]");
            } 
            else 
            {
                AnsiConsole.MarkupLine(":check_mark: [green]Already pre-built![/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine(":check_mark: [green]Build skipped![/]");
        }

        // 3. simulate
        var stepSize = Math.Round(100.0 / settings.Shots, 2);
        var results = new Dictionary<string, int>();

        var simulateCommandLineInfo = simulation.GetSimulateCommandLineInfo(settings);

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
                        var (standardOutput, standardError) = await SimpleExec.Command.ReadAsync(simulateCommandLineInfo.Name, args: simulateCommandLineInfo.Args);

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

        //var results = await simulation.Simulate(settings);

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
}