namespace Collapse;

public static class OutputExtensions
{
    public static string SanitizeOutput(this string standardOutput)
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