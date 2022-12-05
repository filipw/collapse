namespace Collapse;

public record CommandLineInfo
{
    public static CommandLineInfo None = new();

    public string Name { get; init; }
    public string Args { get; init; }
}
