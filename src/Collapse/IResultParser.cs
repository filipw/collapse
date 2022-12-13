namespace Collapse;

public interface IResultParser
{
    public Dictionary<string, double> ParseResults(string raw);
}
