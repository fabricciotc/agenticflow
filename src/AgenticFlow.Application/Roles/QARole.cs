using AgenticFlow.Application.Abstractions;

namespace AgenticFlow.Application.Roles;

/// <summary>
/// QA role that reviews implementation output and stores the verdict in memory.
/// </summary>
public class QARole : Role
{
    private readonly Actions.ReviewAction _reviewAction;

    public QARole(Actions.ReviewAction reviewAction)
    {
        Name = "qa";
        _reviewAction = reviewAction;
    }

    public override bool HasPendingWork(IContext context) => true;

    public override async Task RunAsync(IContext context, CancellationToken cancellationToken = default)
    {
        var message = await _reviewAction.RunAsync(context, cancellationToken);
        context.Memory.Add(message);
    }
}
