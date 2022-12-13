using System.Text.Json;

namespace Collapse;

public class AzureResultParser : IResultParser
{
    public Dictionary<string, double> ParseResults(string result)
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