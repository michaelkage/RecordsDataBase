using System.IO;

namespace BombiHighSchool.App.Services;

public sealed class BackupService
{
    private readonly string _backupDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BombiHighSchool", "Backups");

    public BackupService() => Directory.CreateDirectory(_backupDirectory);

    public string BackupDirectory => _backupDirectory;

    public IReadOnlyList<string> GetRecentBackups(int max = 10) =>
        Directory.EnumerateFiles(_backupDirectory, "*.bak")
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .Take(max)
            .ToList();

    public async Task<string> CreateBackupAsync(string sourceDatabasePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(sourceDatabasePath)) throw new FileNotFoundException("The database file could not be found.", sourceDatabasePath);
        var target = Path.Combine(_backupDirectory, $"BombiHighSchool_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
        await using var source = new FileStream(sourceDatabasePath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, useAsync: true);
        await using var destination = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, useAsync: true);
        await source.CopyToAsync(destination, cancellationToken);
        return target;
    }
}
