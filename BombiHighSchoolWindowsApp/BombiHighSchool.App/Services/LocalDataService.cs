using System.Text.Json;
using BombiHighSchool.App.Models;
using Windows.Storage;

namespace BombiHighSchool.App.Services;

/// <summary>
/// Owns the Windows app's private local database.
/// This data is intentionally separate from the website database/API.
/// </summary>
public sealed class LocalDataService
{
    private const string DatabaseFileName = "database.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _databasePath;

    public LocalDataService()
    {
        var localFolder = ApplicationData.Current.LocalFolder.Path;
        _databasePath = Path.Combine(localFolder, DatabaseFileName);
    }

    public string DatabasePath => _databasePath;

    public async Task<SchoolData> LoadAsync()
    {
        if (!File.Exists(_databasePath))
        {
            var data = new SchoolData();
            await SaveAsync(data);
            return data;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_databasePath);
            return JsonSerializer.Deserialize<SchoolData>(json, JsonOptions) ?? new SchoolData();
        }
        catch (JsonException)
        {
            // Never destroy the user's local data because of malformed JSON.
            // The caller can decide how to surface the problem to the user.
            return new SchoolData();
        }
    }

    public async Task SaveAsync(SchoolData data)
    {
        var json = JsonSerializer.Serialize(data, JsonOptions);
        var tempPath = _databasePath + ".tmp";

        await File.WriteAllTextAsync(tempPath, json);

        File.Move(tempPath, _databasePath, overwrite: true);
    }
}