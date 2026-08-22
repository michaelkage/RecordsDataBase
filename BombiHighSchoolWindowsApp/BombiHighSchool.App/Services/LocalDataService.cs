using System.Text.Json;
using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public class LocalDataService
{
    private readonly string _databasePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public LocalDataService()
    {
        var appFolder = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "BombiHighSchool"
        );

        Directory.CreateDirectory(appFolder);

        _databasePath = Path.Combine(
            appFolder,
            "database.json"
        );
    }

    public async Task<SchoolData> LoadAsync()
    {
        if (!File.Exists(_databasePath))
        {
            var data = new SchoolData();

            await SaveAsync(data);

            return data;
        }

        var json = await File.ReadAllTextAsync(_databasePath);

        return JsonSerializer.Deserialize<SchoolData>(
            json,
            JsonOptions
        ) ?? new SchoolData();
    }

    public async Task SaveAsync(SchoolData data)
    {
        var json = JsonSerializer.Serialize(
            data,
            JsonOptions
        );

        await File.WriteAllTextAsync(
            _databasePath,
            json
        );
    }
}