namespace AgenticFlow.Application.Abstractions;

public interface IAIRunner
{
    string BackendId { get; }
    int Priority { get; }
    bool IsAvailable();
    Task<string> InvokeAsync(string prompt, CancellationToken cancellationToken = default);
}
