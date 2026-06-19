using AgenticFlow.Application.Abstractions;
using AgenticFlow.Domain.Entities;

namespace AgenticFlow.Application.Actions;

/// <summary>
/// Reviews a completed task and returns approval or rejection feedback.
/// </summary>
public class ReviewAction : Action
{
    private readonly IAIRunner _runner;

    public ReviewAction(IAIRunner runner)
    {
        _runner = runner;
    }

    public override async Task<Message> RunAsync(IContext context, CancellationToken cancellationToken = default)
    {
        var prompt = "Review the implementation and provide feedback or approval.";
        var result = await _runner.InvokeAsync(prompt, cancellationToken);
        return new Message
        {
            Role = "qa",
            Type = "review",
            Content = result,
            Cause = context.Ticket.Id.ToString()
        };
    }
}
