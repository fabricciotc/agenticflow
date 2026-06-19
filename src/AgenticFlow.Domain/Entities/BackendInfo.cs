namespace AgenticFlow.Domain.Entities;

public class BackendInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Available { get; set; }
    public int Priority { get; set; }
}
