using System.Text.Json;

namespace BombiHighSchool.App.Services;

public sealed record AuditEntry(DateTime Timestamp, string Action, string Details);

public sealed class AuditLogService
{
    private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BombiHighSchool", "audit-log.json");
    private readonly object _sync = new();

    public AuditLogService()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
    }

    public void Record(string action, string details)
    {
        lock (_sync)
        {
            var entries = Read().ToList();
            entries.Insert(0, new AuditEntry(DateTime.Now, action, details));
            File.WriteAllText(_path, JsonSerializer.Serialize(entries.Take(500), new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    public IReadOnlyList<AuditEntry> GetRecent(int count = 50) => Read().Take(count).ToList();

    private IEnumerable<AuditEntry> Read()
    {
        try
        {
            if (!File.Exists(_path)) return [];
            return JsonSerializer.Deserialize<List<AuditEntry>>(File.ReadAllText(_path)) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
