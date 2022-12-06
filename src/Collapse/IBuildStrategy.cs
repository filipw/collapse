namespace Collapse;

public interface IBuildStrategy
{
    CommandLineInfo GetBuildCommandLineInfo(string path);
}
