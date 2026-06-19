using AgenticFlow.Application.Abstractions;

namespace AgenticFlow.Application.Roles;

/// <summary>
/// Dispatcher support role responsible for routing work to other roles.
/// </summary>
public class DispatcherRole : Role
{
    public DispatcherRole() => Name = "dispatcher";

    public override bool HasPendingWork(IContext context) => false;

    public override Task RunAsync(IContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
