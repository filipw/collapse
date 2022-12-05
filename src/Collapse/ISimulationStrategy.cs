namespace Collapse;

public interface ISimulationStrategy
{
    CommandLineInfo GetBuildCommandLineInfo(string path);
    CommandLineInfo GetSimulateCommandLineInfo(SimulateCommandSettings settings);
}
