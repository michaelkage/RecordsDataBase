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
        await FileLock.WaitAsync();
        try { LastLoadWarning = null; return await LoadUnlockedAsync(); }
        finally { FileLock.Release(); }
    }
    public async Task UpdateAsync(Func<SchoolData, Task> mutation)
    {
        ArgumentNullException.ThrowIfNull(mutation);
        await FileLock.WaitAsync();
        try { LastLoadWarning = null; var data = await LoadUnlockedAsync(); await mutation(data); DatabaseIntegrityValidator.Validate(data); await SaveUnlockedAsync(data, true); }
        finally { FileLock.Release(); }
    }
    private async Task<SchoolData> LoadUnlockedAsync()
    {
        if (!File.Exists(_databasePath)) { var data = new SchoolData(); await SaveUnlockedAsync(data, false); return data; }
        try
        {
            var data = JsonSerializer.Deserialize<SchoolData>(await File.ReadAllTextAsync(_databasePath), JsonOptions) ?? throw new JsonException("Database is empty.");
            DatabaseIntegrityValidator.Validate(data); return data;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException or DatabaseIntegrityException)
        {
            if (!File.Exists(_backupPath)) throw new DatabaseUnavailableException("The local school database could not be read safely and no valid backup exists.", ex);
            try
            {
                var recovered = JsonSerializer.Deserialize<SchoolData>(await File.ReadAllTextAsync(_backupPath), JsonOptions) ?? throw new JsonException("The backup is empty.");
                DatabaseIntegrityValidator.Validate(recovered);
                TryCopy(_databasePath, _databasePath + ".corrupt-" + DateTime.Now.ToString("yyyyMMdd-HHmmssfff") + ".json");
                await ReplaceFromDataAsync(recovered, ".recovery.tmp");
                CleanupTemporaryFiles();
                LastLoadWarning = "The active database was damaged. The latest valid backup was restored and the damaged file was preserved.";
                return recovered;
            }
            catch (Exception recoveryEx) when (recoveryEx is JsonException or IOException or UnauthorizedAccessException or DatabaseIntegrityException)
            { throw new DatabaseUnavailableException("The local school database and its backup could not be read safely.", recoveryEx); }
        }
    }
    public async Task SaveAsync(SchoolData data)
    {
        DatabaseIntegrityValidator.Validate(data); await FileLock.WaitAsync();
        try { await SaveUnlockedAsync(data, true); } finally { FileLock.Release(); }
    }
    private async Task SaveUnlockedAsync(SchoolData data, bool createBackup)
    {
        DatabaseIntegrityValidator.Validate(data); var tempPath = _databasePath + ".tmp";
        try
        {
            await File.WriteAllTextAsync(tempPath, JsonSerializer.Serialize(data, JsonOptions));
            if (createBackup && File.Exists(_databasePath)) { File.Copy(_databasePath, _backupPath, true); ArchiveBackup(_databasePath); }
            File.Move(tempPath, _databasePath, true);
        }
        catch { TryDelete(tempPath); throw; }
    }
    private async Task ReplaceFromDataAsync(SchoolData data, string suffix)
    {
        var tempPath = _databasePath + suffix;
        try { await File.WriteAllTextAsync(tempPath, JsonSerializer.Serialize(data, JsonOptions)); File.Move(tempPath, _databasePath, true); }
        catch { TryDelete(tempPath); throw; }
    }
    private void ArchiveBackup(string sourcePath)
    {
        try
        {
            Directory.CreateDirectory(_backupDirectory);
            File.Copy(sourcePath, Path.Combine(_backupDirectory, $"database-{DateTime.Now:yyyyMMdd-HHmmssfff}.json"), false);
            foreach (var file in Directory.GetFiles(_backupDirectory, "database-*.json").OrderByDescending(File.GetCreationTimeUtc).Skip(MaxArchivedBackups)) TryDelete(file);
        }
        catch { }
    }
    private void CleanupTemporaryFiles()
    {
        foreach (var suffix in new[] { ".tmp", ".recovery.tmp", ".restore.tmp", ".import.tmp" }) TryDelete(_databasePath + suffix);
    }
    private static void TryDelete(string path) { try { if (File.Exists(path)) File.Delete(path); } catch { } }
    private static void TryCopy(string source, string destination) { try { if (File.Exists(source)) File.Copy(source, destination, true); } catch { } }

    public async Task CreateBackupAsync()
    {
        await FileLock.WaitAsync();
        try { if (!File.Exists(_databasePath)) throw new InvalidOperationException("There is no local database to back up yet."); var data = await LoadUnlockedAsync(); DatabaseIntegrityValidator.Validate(data); File.Copy(_databasePath, _backupPath, true); ArchiveBackup(_databasePath); }
        finally { FileLock.Release(); }
    }
    public async Task RestoreBackupAsync()
    {
        await FileLock.WaitAsync();
        try
        {
            if (!File.Exists(_backupPath)) throw new FileNotFoundException("No local backup exists yet.");
            var backup = JsonSerializer.Deserialize<SchoolData>(await File.ReadAllTextAsync(_backupPath), JsonOptions) ?? throw new JsonException("The backup is empty.");
            DatabaseIntegrityValidator.Validate(backup);
            if (File.Exists(_databasePath)) { TryCopy(_databasePath, _databasePath + ".before-restore-" + DateTime.Now.ToString("yyyyMMdd-HHmmssfff") + ".json"); ArchiveBackup(_databasePath); }
            await ReplaceFromDataAsync(backup, ".restore.tmp"); CleanupTemporaryFiles();
        }
        finally { FileLock.Release(); }
    }
    public async Task ExportDatabaseAsync(string destinationPath)
    {
        if (string.IsNullOrWhiteSpace(destinationPath)) throw new ArgumentException("A destination path is required.", nameof(destinationPath));
        var destination = Path.GetFullPath(destinationPath); await FileLock.WaitAsync();
        try { var data = await LoadUnlockedAsync(); if (string.Equals(destination, _databasePath, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("The export destination cannot be the active database file."); DatabaseIntegrityValidator.Validate(data); File.Copy(_databasePath, destination, true); }
        finally { FileLock.Release(); }
    }
    public async Task ImportDatabaseAsync(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath)) throw new ArgumentException("A source path is required.", nameof(sourcePath));
        var source = Path.GetFullPath(sourcePath); await FileLock.WaitAsync();
        try
        {
            if (!File.Exists(source)) throw new FileNotFoundException("The selected database file does not exist.");
            if (string.Equals(source, _databasePath, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("The selected file is already the active database.");
            var imported = JsonSerializer.Deserialize<SchoolData>(await File.ReadAllTextAsync(source), JsonOptions) ?? throw new JsonException("The selected file is not a valid Bombi High School database.");
            DatabaseIntegrityValidator.Validate(imported);
            if (File.Exists(_databasePath)) { File.Copy(_databasePath, _backupPath, true); ArchiveBackup(_databasePath); }
            await ReplaceFromDataAsync(imported, ".import.tmp"); CleanupTemporaryFiles();
        }
        finally { FileLock.Release(); }
    }
}

public sealed class DatabaseUnavailableException : Exception
{
    public DatabaseUnavailableException(string message, Exception inner) : base(message, inner) { }
}
