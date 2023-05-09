using System.Text.Json;

namespace Collapse;

public static class OutputParser
{
    private static IReadOnlyDictionary<string, double> Empty = new Dictionary<string, double>();

    public static IReadOnlyDictionary<string, double> ParseResults(string standardOutput)
    {
        var from = standardOutput.IndexOf("{");
        if (from == -1) return Empty;

        var to = standardOutput.LastIndexOf("}") + 1;
        if (to == -1) return Empty;
        
        var json = standardOutput[from..to];
        try
        {
            var deserialized = JsonSerializer.Deserialize<AzureExecutionResponse>(json);

            var results = new Dictionary<string, double>();
            for (var i = 0; i < deserialized.Histogram.Length; i += 2)
            {
                if (deserialized.Histogram[i].ValueKind == JsonValueKind.String && deserialized.Histogram[i + 1].ValueKind == JsonValueKind.Number)
                {
                    var label = deserialized.Histogram[i].GetString();
                    var data = deserialized.Histogram[i + 1].GetDouble();
                    results[SanitizeOutput(label)] = data;
                }
                else
                {
                    // todo: log this
                }
            }

            return results;
        } 
        catch (Exception)
        {
            // todo: log this
            return Empty;
        }
    }

    public static string SanitizeOutput(string standardOutput, bool isQirRunner = false)
    {
        // only take the last line, because previous lines might contain any stdio output of the program itself
        var rawResultLines = standardOutput.Trim().Split(Environment.NewLine);
        var rawResult = isQirRunner ? rawResultLines[^2] : rawResultLines[^1];
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