using AgenticFlow.Application.Abstractions;

namespace AgenticFlow.Application.Roles;

/// <summary>
/// Monitor support role responsible for tracking system health and progress.
/// </summary>
public class MonitorRole : Role
{
    public MonitorRole() => Name = "monitor";

    public override bool HasPendingWork(IContext context) => false;

    public override Task RunAsync(IContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
