using System.Text.Json;

namespace Collapse;

public class OutputParser
{
    public static Dictionary<string, double> ParseResults(string standardOutput)
    {
        var from = standardOutput.IndexOf("{");
        var to = standardOutput.LastIndexOf("}") + 1;
        var json = standardOutput[from..to];

        var deserialized = JsonSerializer.Deserialize<AzureExecutionResponse>(json);

        var results = new Dictionary<string, double>();
        for (var i = 0; i < deserialized.Histogram.Length; i+=2)
        {
            if (deserialized.Histogram[i].ValueKind == JsonValueKind.String && deserialized.Histogram[i + 1].ValueKind == JsonValueKind.Number)
            {
                var label = deserialized.Histogram[i].GetString();
                var data = deserialized.Histogram[i + 1].GetDouble();
                results[SanitizeOutput(label)] = data;
            } else {
                // todo: log this
            }
        }

        return results;
    }

    public static string SanitizeOutput(string standardOutput)
    {
        // only take the last line, because previous lines might contain any stdio output of the program itself
        var rawResult = standardOutput.Trim().Split(Environment.NewLine).LastOrDefault();
        if (rawResult == "Zero") return "|0⟩";
        if (rawResult == "One") return "|1⟩";
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