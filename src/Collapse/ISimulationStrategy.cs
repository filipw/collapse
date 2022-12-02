namespace Collapse;

public interface ISimulationStrategy
{
    Task<Dictionary<string, int>> Simulate(SimulateCommandSettings settings);
}
