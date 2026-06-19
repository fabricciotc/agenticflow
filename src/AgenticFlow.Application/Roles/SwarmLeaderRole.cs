using AgenticFlow.Application.Abstractions;

namespace AgenticFlow.Application.Roles;

/// <summary>
/// Swarm leader support role that decomposes large tasks into parallel subtasks.
/// </summary>
public class SwarmLeaderRole : Role
{
    public SwarmLeaderRole() => Name = "swarm_leader";

    public override bool HasPendingWork(IContext context) => false;

    public override Task RunAsync(IContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
