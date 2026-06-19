using AgenticFlow.Application.Abstractions;

namespace AgenticFlow.Application.Roles;

/// <summary>
/// Aggregator support role that collects subtask outputs and emits a single completion.
/// </summary>
public class AggregatorRole : Role
{
    public AggregatorRole() => Name = "aggregator";

    public override bool HasPendingWork(IContext context) => false;

    public override Task RunAsync(IContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
