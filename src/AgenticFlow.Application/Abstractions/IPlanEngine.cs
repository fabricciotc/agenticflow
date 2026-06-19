using AgenticFlow.Domain.Entities;

namespace AgenticFlow.Application.Abstractions;

public interface IPlanEngine
{
    Task<Plan> BuildPlanAsync(Ticket ticket, CancellationToken cancellationToken = default);
}

public class Plan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public List<TaskItem> Tasks { get; set; } = new();
}

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public List<Guid> Dependencies { get; set; } = new();
    public string Assignee { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}
