namespace AgenticFlow.Domain.Entities;

public class Plan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public List<TaskItem> Tasks { get; set; } = new();
}
