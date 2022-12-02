namespace Collapse;

public interface ISimulationStrategy
{
    CommandLineInfo GetSimulateCommandLineInfo(SimulateCommandSettings settings);
}
