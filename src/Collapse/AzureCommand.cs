using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Collapse;

internal sealed class AzureCommand : AsyncCommand<AzureCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, AzureCommandSettings settings)
    {
        AnsiConsole.WriteLine();

        // 1. choose strategy
        var strategy = new AzureSyncExecutionStrategy(settings);
        var commandLineInfo = strategy.GetExecuteCommandLineInfo(settings.Path);

        string result = null;
        // 2. run
        await AnsiConsole.Status()
        .StartAsync("[yellow]Waiting for Azure Quantum...[/]", async ctx =>
        {
            var (standardOutput, standardError) = await SimpleExec.Command.ReadAsync(commandLineInfo.Name, args: commandLineInfo.Args);

            AnsiConsole.MarkupLine(":check_mark: [green]Executed successfully![/]");
            result = standardOutput;
        });

        await Task.Delay(500);

        // 3. parse
        var results = OutputParser.ParseResults(result);
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
}
