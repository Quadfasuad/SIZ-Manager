using System.IO;
using SizManager.Helpers;

namespace SizManager.Services;

public class BackupService
{
    private const int MaxBackups = 5;

    public async Task<string> CreateBackupAsync()
    {
        var dbPath = AppPaths.DatabasePath;
        if (!File.Exists(dbPath))
            throw new FileNotFoundException("База данных не найдена", dbPath);

        Directory.CreateDirectory(AppPaths.BackupsDir);

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var backupName = $"database_backup_{timestamp}.db";
        var backupPath = Path.Combine(AppPaths.BackupsDir, backupName);

        await Task.Run(() => File.Copy(dbPath, backupPath, overwrite: true));

        await CleanOldBackupsAsync();

        return backupPath;
    }

    public async Task RestoreBackupAsync(string backupPath)
    {
        if (!File.Exists(backupPath))
            throw new FileNotFoundException("Файл резервной копии не найден", backupPath);

        var dbPath = AppPaths.DatabasePath;
        await Task.Run(() => File.Copy(backupPath, dbPath, overwrite: true));
    }

    public Task<List<string>> GetBackupsAsync()
    {
        return Task.Run(() =>
        {
            if (!Directory.Exists(AppPaths.BackupsDir))
                return new List<string>();

            return Directory.GetFiles(AppPaths.BackupsDir, "database_backup_*.db")
                .OrderByDescending(f => f)
                .ToList();
        });
    }

    private Task CleanOldBackupsAsync()
    {
        return Task.Run(() =>
        {
            var backups = Directory.GetFiles(AppPaths.BackupsDir, "database_backup_*.db")
                .OrderByDescending(f => f)
                .ToArray();

            foreach (var old in backups.Skip(MaxBackups))
            {
                try { File.Delete(old); }
                catch { /* ignore */ }
            }
        });
    }
}
