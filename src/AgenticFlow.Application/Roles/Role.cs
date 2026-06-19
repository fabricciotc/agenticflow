using AgenticFlow.Application.Abstractions;

namespace AgenticFlow.Application.Roles;

public abstract class Role
{
    public string Name { get; protected set; } = string.Empty;

    public abstract bool HasPendingWork(IContext context);
    public abstract Task RunAsync(IContext context, CancellationToken cancellationToken = default);
}
