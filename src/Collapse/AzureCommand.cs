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

        var results = ParseResult(result);
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

    private Dictionary<string, double> ParseResult(string result)
    {
        var from = result.IndexOf("{");
        var to = result.LastIndexOf("}") + 1;
        var json = result[from..to];

        var deserialized = JsonSerializer.Deserialize<AzureExecutionResponse>(json);

        // AnsiConsole.WriteLine(json);

        var results = new Dictionary<string, double>();
        for (var i = 0; i < deserialized.Histogram.Length; i+=2)
        {
            if (deserialized.Histogram[i].ValueKind == JsonValueKind.String && deserialized.Histogram[i + 1].ValueKind == JsonValueKind.Number)
            {
                var label = deserialized.Histogram[i].GetString();
                var data = deserialized.Histogram[i + 1].GetDouble();
                results[label.SanitizeOutput()] = data;
            } else {
                // todo: log this
            }
        }

        return results;
    }
}
