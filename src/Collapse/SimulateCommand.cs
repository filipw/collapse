using Spectre.Console;
using System.Runtime.InteropServices;
using Spectre.Console.Cli;

namespace Collapse;

internal sealed class SimulateCommand : AsyncCommand<SimulateCommandSettings>
{
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

        ISimulationStrategy simulation = settings.Qir ? new QirSimulationStrategy() : new DotnetSimulationStrategy();
        var results = await simulation.Simulate(settings);

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