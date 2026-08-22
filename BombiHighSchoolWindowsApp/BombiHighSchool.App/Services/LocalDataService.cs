using System.Text.Json;
using BombiHighSchool.App.Models;

namespace BombiHighSchool.App.Services;

public class LocalDataService
{
    private readonly string _dataDirectory;
    private readonly string _databasePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public LocalDataService()
    {
        _dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BombiHighSchool"
        );

        _databasePath = Path.Combine(_dataDirectory, "database.json");
    }

    public async Task<SchoolData> LoadAsync()
    {
        Directory.CreateDirectory(_dataDirectory);

        if (!File.Exists(_databasePath))
        {
            var data = CreateDefaultData();
            await SaveAsync(data);
            return data;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_databasePath);

            return JsonSerializer.Deserialize<SchoolData>(
                json,
                JsonOptions
            ) ?? CreateDefaultData();
        }
        catch
        {
            // If the file is damaged or unreadable,
            // start with a fresh database.
            return CreateDefaultData();
        }
    }

    public async Task SaveAsync(SchoolData data)
    {
        Directory.CreateDirectory(_dataDirectory);

        var json = JsonSerializer.Serialize(
            data,
            JsonOptions
        );

        await File.WriteAllTextAsync(
            _databasePath,
            json
        );
    }

    private static SchoolData CreateDefaultData()
    {
        return new SchoolData
        {
            Admin = new AdminAccount
            {
                Password = "admin"
            },

            Students = [],

            Subjects =
            [
                "Mathematics",
                "English Language",
                "Physics",
                "Chemistry",
                "Biology",
                "Computer Science"
            ]
        };
    }
}