namespace AgenticFlow.Application.Abstractions;

public interface IBackendRegistry
{
    IReadOnlyList<IAIRunner> GetAvailableRunners();
    IAIRunner? GetRunner(string backendId);
}
