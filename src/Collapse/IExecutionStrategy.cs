namespace Collapse;

public interface IExecutionStrategy
{
    CommandLineInfo GetExecuteCommandLineInfo(string path);
}
