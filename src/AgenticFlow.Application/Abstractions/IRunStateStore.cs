namespace AgenticFlow.Application.Abstractions;

public interface IRunStateStore
{
    RunState Load();
    void Save(RunState state);
}

public class RunState
{
    public string Status { get; set; } = "idle";
    public Dictionary<string, object> Data { get; set; } = new();
}
