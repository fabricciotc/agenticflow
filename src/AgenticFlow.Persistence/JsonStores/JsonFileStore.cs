using System.Text.Json;
using AgenticFlow.Persistence.FileSystem;

namespace AgenticFlow.Persistence.JsonStores;

public class JsonFileStore<T> where T : class, new()
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options;

    public JsonFileStore(AppDataPathProvider pathProvider, string fileName)
    {
        _filePath = pathProvider.GetFilePath(fileName);
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public T Load()
    {
        if (!File.Exists(_filePath))
        {
            return new T();
        }

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<T>(json, _options) ?? new T();
    }

    public void Save(T value)
    {
        var json = JsonSerializer.Serialize(value, _options);
        File.WriteAllText(_filePath, json);
    }
}
