using AgenticFlow.Application.Abstractions;

namespace AgenticFlow.Application.Roles;

/// <summary>
/// Recovery support role responsible for deciding retries or escalation on failures.
/// </summary>
public class RecoveryRole : Role
{
    public RecoveryRole() => Name = "recovery";

    public override bool HasPendingWork(IContext context) => false;

    public override Task RunAsync(IContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
