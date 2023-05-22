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
                    results[SanitizeOutput(label)[0]] = data;
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

    public static string[] SanitizeOutput(string standardOutput, bool isQirRunner = false)
    {
        string[] rawResultLines;
        if (!isQirRunner)
        {
            // only take the last line, because previous lines might contain any stdio output of the program itself
            rawResultLines = standardOutput.Trim().Split(Environment.NewLine)[^1..];
        } else {
            rawResultLines = standardOutput.Trim().Split(Environment.NewLine).Where(line => 
                !line.StartsWith("METADATA") && 
                !line.StartsWith("START") && 
                !line.StartsWith("END")).ToArray();
        }

        for (var i = 0; i<rawResultLines.Length; i++)
        {
            if (rawResultLines[i] == "Zero") rawResultLines[i] = "|0⟩";
            if (rawResultLines[i] == "One") rawResultLines[i] = "|1⟩";
            rawResultLines[i] = rawResultLines[i].
                Replace("(", "|").
                Replace("[", "|").
                Replace(")", "⟩").
                Replace("]", "⟩").
                Replace(" ", string.Empty).
                Replace(",", string.Empty).
                Replace("Zero", "0").
                Replace("One", "1");
        }

        return rawResultLines;
    }
}