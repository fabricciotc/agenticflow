using AgenticFlow.Application.Abstractions;
using AgenticFlow.Domain.Entities;

namespace AgenticFlow.Application.Actions;

public abstract class Action
{
    public abstract Task<Message> RunAsync(IContext context, CancellationToken cancellationToken = default);
}
