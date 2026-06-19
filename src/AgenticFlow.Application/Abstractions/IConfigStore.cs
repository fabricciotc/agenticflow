namespace AgenticFlow.Application.Abstractions;

public interface IConfigStore
{
    AppConfig Load();
    void Save(AppConfig config);
}

public class AppConfig
{
    public string Backend { get; set; } = string.Empty;
    public int MaxWorkers { get; set; } = 4;
    public string ProjectsRoot { get; set; } = string.Empty;
}
