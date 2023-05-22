using Spectre.Console;
using System.Runtime.InteropServices;
using Spectre.Console.Cli;

namespace Collapse;

internal sealed class SimulateCommand : AsyncCommand<SimulateCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, SimulateCommandSettings settings)
    {
        // 1. choose strategy
        ISimulationStrategy simulation = settings.Qir ? new QirSimulationStrategy(settings) : new DotnetSimulationStrategy(settings);

        // 2. build
        if (!settings.SkipBuild)
        {
            var buildCommandLineInfo = simulation.GetBuildCommandLineInfo(settings.Path);
            if (buildCommandLineInfo != CommandLineInfo.None)
            {
                await AnsiConsole.Status()
                    .StartAsync($"[yellow]{buildCommandLineInfo.Title}...[/]", async ctx =>
                    {
                        var (standardOutput, standardError) = await SimpleExec.Command.ReadAsync(buildCommandLineInfo.Name, args: buildCommandLineInfo.Args);
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

        // 3. simulate and parse
        var results = new SortedDictionary<string, int>();
        var simulateCommandLineInfo = simulation.GetExecuteCommandLineInfo(settings.Path);

        if (settings.NoOrchestration) 
        {
            await RunShots(simulateCommandLineInfo.Name, simulateCommandLineInfo.Args, settings.Qir, results);
        } 
        else 
        {
            var stepSize = Math.Round(100.0 / settings.Shots, 2);
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
                    for (int i = 0; i < settings.Shots; i++)
                    {
                        await RunShots(simulateCommandLineInfo.Name, simulateCommandLineInfo.Args, settings.Qir, results);
                        shotTask.Increment(stepSize);
                    }

                    shotTask.Value = 100;
                });
        }

        AnsiConsole.MarkupLine(":check_mark: [green]Finished running shots![/]");
        AnsiConsole.WriteLine();

        var chart = new BarChart()
            .Width(60)
            .Label("[green]Results:[/]");

        for (var i = 0; i < results.Count; i++)
        {
            var color = PreferredColors.Entries.Length > i ? PreferredColors.Entries[i] : Color.White;
            chart.AddItem(results.ElementAt(i).Key.EscapeMarkup(), results.ElementAt(i).Value, color);
        }

        AnsiConsole.Write(chart);

        return 0;
    }

    private static async Task RunShots(string command, string args, bool qir, SortedDictionary<string, int> results)
    {
        var (standardOutput, standardError) = await SimpleExec.Command.ReadAsync(command, args: args);

        var sanitizedResults = OutputParser.SanitizeOutput(standardOutput, qir);

        foreach (var result in sanitizedResults)
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
    }
}
