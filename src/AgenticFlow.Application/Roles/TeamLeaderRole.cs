using AgenticFlow.Application.Abstractions;

namespace AgenticFlow.Application.Roles;

/// <summary>
/// Team leader support role that coordinates workers.
/// </summary>
public class TeamLeaderRole : Role
{
    public TeamLeaderRole() => Name = "team_leader";

    public override bool HasPendingWork(IContext context) => false;

    public override Task RunAsync(IContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
