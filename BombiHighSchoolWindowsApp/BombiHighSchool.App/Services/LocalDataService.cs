using System.Text.Json;
using BombiHighSchool.App.Models;
using Windows.Storage;

namespace BombiHighSchool.App.Services;

public sealed class LocalDataService
{
    private const string DatabaseFileName = "database.json";
    private const string BackupFileName = "database.backup.json";
    private const int MaxArchivedBackups = 10;
    private static readonly SemaphoreSlim FileLock = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    private readonly string _databasePath;
    private readonly string _backupPath;
    private readonly string _backupDirectory;

    public LocalDataService()
    {
        var folder = ApplicationData.Current.LocalFolder.Path;
        _databasePath = Path.Combine(folder, DatabaseFileName);
        _backupPath = Path.Combine(folder, BackupFileName);
        _backupDirectory = Path.Combine(folder, "Backups");
        Directory.CreateDirectory(_backupDirectory);
    }

    public string DatabasePath => _databasePath;
    public string BackupPath => _backupPath;
    public string? LastLoadWarning { get; private set; }

    public async Task<SchoolData> LoadAsync()
    {
        LastLoadWarning = null;
        await FileLock.WaitAsync();
        try { return await LoadUnlockedAsync(); }
        finally { FileLock.Release(); }
    }

    public async Task UpdateAsync(Func<SchoolData, Task> mutation)
    {
        ArgumentNullException.ThrowIfNull(mutation);
        await FileLock.WaitAsync();
        try
        {
            LastLoadWarning = null;
            var data = await LoadUnlockedAsync();
            await mutation(data);
            DatabaseIntegrityValidator.Validate(data);
            await SaveUnlockedAsync(data, true);
        }
        finally { FileLock.Release(); }
    }

    private async Task<SchoolData> LoadUnlockedAsync()
    {
        if (!File.Exists(_databasePath))
        {
            var data = new SchoolData();
            await SaveUnlockedAsync(data, false);
            return data;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_databasePath);
            var data = JsonSerializer.Deserialize<SchoolData>(json, JsonOptions) ?? throw new JsonException("Database is empty.");
            DatabaseIntegrityValidator.Validate(data);
            return data;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or DatabaseIntegrityException)
        {
            if (File.Exists(_backupPath))
            {
                try
                {
                    var backupJson = await File.ReadAllTextAsync(_backupPath);
                    var recovered = JsonSerializer.Deserialize<SchoolData>(backupJson, JsonOptions);
                    if (recovered is not null)
                    {
                        DatabaseIntegrityValidator.Validate(recovered);
                        var corruptCopy = _databasePath + ".corrupt-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".json";
                        try { File.Copy(_databasePath, corruptCopy, true); } catch { }
                        LastLoadWarning = "The active database was damaged. Bombi High School recovered the latest valid backup and preserved the damaged file. Review Settings → Database management.";
                        return recovered;
                    }
                }
                catch { }
            }
            throw new DatabaseUnavailableException("The local school database could not be read or failed integrity checks. Your data was not replaced with an empty database. Use Settings → Restore backup or Import database.", ex);
        }
    }

    public async Task SaveAsync(SchoolData data)
    {
        DatabaseIntegrityValidator.Validate(data);
        await FileLock.WaitAsync();
        try { await SaveUnlockedAsync(data, true); }
        finally { FileLock.Release(); }
    }

    private async Task SaveUnlockedAsync(SchoolData data, bool createBackup)
    {
        DatabaseIntegrityValidator.Validate(data);
        var json = JsonSerializer.Serialize(data, JsonOptions);
        var tempPath = _databasePath + ".tmp";
        await File.WriteAllTextAsync(tempPath, json);

        if (createBackup && File.Exists(_databasePath))
        {
            File.Copy(_databasePath, _backupPath, true);
            ArchiveBackup(_databasePath);
        }

        File.Move(tempPath, _databasePath, true);
    }

    private void ArchiveBackup(string sourcePath)
    {
        try
        {
            Directory.CreateDirectory(_backupDirectory);
            var archivePath = Path.Combine(_backupDirectory, $"database-{DateTime.Now:yyyyMMdd-HHmmssfff}.json");
            File.Copy(sourcePath, archivePath, false);
            var old = Directory.GetFiles(_backupDirectory, "database-*.json")
                .OrderByDescending(File.GetCreationTimeUtc)
                .Skip(MaxArchivedBackups)
                .ToList();
            foreach (var file in old) { try { File.Delete(file); } catch { } }
        }
        catch { /* Backup archiving must never block a valid database save. */ }
    }

    public async Task CreateBackupAsync()
    {
        await FileLock.WaitAsync();
        try
        {
            if (!File.Exists(_databasePath)) throw new InvalidOperationException("There is no local database to back up yet.");
            var data = await LoadUnlockedAsync();
            DatabaseIntegrityValidator.Validate(data);
            File.Copy(_databasePath, _backupPath, true);
            ArchiveBackup(_databasePath);
        }
        finally { FileLock.Release(); }
    }

    public async Task RestoreBackupAsync()
    {
        await FileLock.WaitAsync();
        try
        {
            if (!File.Exists(_backupPath)) throw new FileNotFoundException("No local backup exists yet.");
            var backupJson = await File.ReadAllTextAsync(_backupPath);
            var backup = JsonSerializer.Deserialize<SchoolData>(backupJson, JsonOptions) ?? throw new JsonException("The backup is empty.");
            DatabaseIntegrityValidator.Validate(backup);

            if (File.Exists(_databasePath))
            {
                File.Copy(_databasePath, _databasePath + ".before-restore-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".json", true);
                ArchiveBackup(_databasePath);
            }

            var tempPath = _databasePath + ".restore.tmp";
            await File.WriteAllTextAsync(tempPath, JsonSerializer.Serialize(backup, JsonOptions));
            File.Move(tempPath, _databasePath, true);
            _ = await LoadUnlockedAsync();
        }
        finally { FileLock.Release(); }
    }

    public async Task ExportDatabaseAsync(string destinationPath)
    {
        await FileLock.WaitAsync();
        try
        {
            if (!File.Exists(_databasePath)) throw new InvalidOperationException("There is no local database to export.");
            var data = await LoadUnlockedAsync();
            DatabaseIntegrityValidator.Validate(data);
            File.Copy(_databasePath, destinationPath, true);
        }
        finally { FileLock.Release(); }
    }

    public async Task ImportDatabaseAsync(string sourcePath)
    {
        await FileLock.WaitAsync();
        try
        {
            if (!File.Exists(sourcePath)) throw new FileNotFoundException("The selected database file does not exist.");
            var json = await File.ReadAllTextAsync(sourcePath);
            var imported = JsonSerializer.Deserialize<SchoolData>(json, JsonOptions) ?? throw new JsonException("The selected file is not a valid Bombi High School database.");
            DatabaseIntegrityValidator.Validate(imported);

            if (File.Exists(_databasePath))
            {
                File.Copy(_databasePath, _backupPath, true);
                ArchiveBackup(_databasePath);
            }

            var tempPath = _databasePath + ".import.tmp";
            await File.WriteAllTextAsync(tempPath, JsonSerializer.Serialize(imported, JsonOptions));
            File.Move(tempPath, _databasePath, true);
        }
        finally { FileLock.Release(); }
    }
}

public sealed class DatabaseUnavailableException : Exception
{
    public DatabaseUnavailableException(string message, Exception inner) : base(message, inner) { }
}
