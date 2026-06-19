namespace AgenticFlow.Application.Abstractions;

public interface IEnvironment
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task RunCycleAsync(CancellationToken cancellationToken = default);
}
