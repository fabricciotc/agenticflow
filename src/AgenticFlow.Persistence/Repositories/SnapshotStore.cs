using System.Text.Json;
using AgenticFlow.Application.Abstractions;
using AgenticFlow.Persistence.FileSystem;
using AgenticFlow.Persistence.JsonStores;

namespace AgenticFlow.Persistence.Repositories;

public class SnapshotStore : ISnapshotStore
{
    private readonly AppDataPathProvider _pathProvider;
    private readonly JsonSerializerOptions _options;

    public SnapshotStore(AppDataPathProvider pathProvider)
    {
        _pathProvider = pathProvider;
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public void Save(string ticketId, Snapshot snapshot)
    {
        var snapshotDir = GetSnapshotDirectory(ticketId);
        Directory.CreateDirectory(snapshotDir);

        var fileName = $"{snapshot.CreatedAt:yyyyMMddHHmmss}-{snapshot.Id}.json";
        var filePath = Path.Combine(snapshotDir, fileName);
        var json = JsonSerializer.Serialize(snapshot, _options);
        File.WriteAllText(filePath, json);
    }

    public Snapshot? Load(string ticketId)
    {
        var snapshotDir = GetSnapshotDirectory(ticketId);
        if (!Directory.Exists(snapshotDir))
        {
            return null;
        }

        var latestFile = Directory
            .EnumerateFiles(snapshotDir, "*.json")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();

        if (latestFile is null)
        {
            return null;
        }

        var json = File.ReadAllText(latestFile);
        return JsonSerializer.Deserialize<Snapshot>(json, _options);
    }

    private string GetSnapshotDirectory(string ticketId)
    {
        var safeTicketId = string.Join("_", ticketId.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_pathProvider.GetAppDataPath(), "snapshots", safeTicketId);
    }
}
