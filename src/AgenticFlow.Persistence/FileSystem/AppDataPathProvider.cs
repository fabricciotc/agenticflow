namespace AgenticFlow.Persistence.FileSystem;

public class AppDataPathProvider
{
    private readonly string _basePath;

    public AppDataPathProvider()
    {
        _basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AgenticFlow");

        Directory.CreateDirectory(_basePath);
    }

    public string GetAppDataPath() => _basePath;

    public string GetFilePath(string fileName) => Path.Combine(_basePath, fileName);
}
