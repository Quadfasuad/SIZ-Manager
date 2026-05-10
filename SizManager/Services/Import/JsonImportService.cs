using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SizManager.Helpers;
using SizManager.Models;
using SizManager.Models.JsonModels;
using SizManager.Services.Database;

namespace SizManager.Services.Import;

public class JsonImportService
{
    private const string EmbeddedResourceName = "SizManager.Resources.siz_database_full.json";
    private readonly BackupService _backupService;

    public JsonImportService(BackupService backupService)
    {
        _backupService = backupService;
    }

    /// <summary>
    /// Import from the embedded JSON resource (for first launch).
    /// </summary>
    public async Task<(int professions, int sizItems)> ImportFromEmbeddedResourceAsync(IProgress<int>? progress = null)
    {
        var data = await ReadEmbeddedDatabaseAsync();
        ValidateData(data);
        return await ImportDataAsync(data, progress);
    }

    public async Task<int> GetEmbeddedProfessionCountAsync()
    {
        var data = await ReadEmbeddedDatabaseAsync();
        ValidateData(data);
        return data.Professions.Count;
    }

    private static async Task<SizDatabase> ReadEmbeddedDatabaseAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(EmbeddedResourceName)
            ?? throw new InvalidOperationException("Встроенный справочник не найден в ресурсах приложения");

        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<SizDatabase>(json)
            ?? throw new InvalidOperationException("Не удалось разобрать встроенный JSON");
    }

    public async Task<(int professions, int sizItems)> ImportAsync(string filePath, IProgress<int>? progress = null)
    {
        // Validate file
        if (!File.Exists(filePath))
            throw new FileNotFoundException("JSON файл не найден", filePath);

        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length > 50 * 1024 * 1024) // 50 MB limit
            throw new InvalidOperationException("Файл слишком большой (максимум 50 МБ)");

        // Read and deserialize
        var json = await File.ReadAllTextAsync(filePath);
        var data = JsonSerializer.Deserialize<SizDatabase>(json)
            ?? throw new InvalidOperationException("Не удалось разобрать JSON файл");

        // Validate structure
        ValidateData(data);

        // Create backup of existing DB before replacing
        if (File.Exists(AppPaths.DatabasePath))
        {
            try { await _backupService.CreateBackupAsync(); }
            catch (Exception ex) { Logger.LogError(ex, "Backup before import"); }
        }

        return await ImportDataAsync(data, progress);
    }

    private async Task<(int professions, int sizItems)> ImportDataAsync(SizDatabase data, IProgress<int>? progress = null)
    {
        int totalSiz = 0;
        using var context = new SizDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // Clear existing profession data
            await context.Database.ExecuteSqlRawAsync("DELETE FROM ProfessionSIZ");
            await context.Database.ExecuteSqlRawAsync("DELETE FROM Professions");

            context.ChangeTracker.AutoDetectChangesEnabled = false;

            int count = 0;
            const int batchSize = 100;

            foreach (var batch in data.Professions.Chunk(batchSize))
            {
                // Add professions
                foreach (var profData in batch)
                {
                    var profession = new Profession
                    {
                        Number = profData.Number,
                        Name = profData.Name,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    context.Professions.Add(profession);
                }

                await context.SaveChangesAsync();

                // Add SIZ items for this batch
                foreach (var profData in batch)
                {
                    var profession = context.Professions.Local
                        .First(p => p.Number == profData.Number);

                    foreach (var sizData in profData.SizList)
                    {
                        context.ProfessionSIZ.Add(new ProfessionSIZ
                        {
                            ProfessionId = profession.Id,
                            Type = sizData.Type,
                            Name = sizData.Name,
                            Norm = sizData.Norm
                        });
                        totalSiz++;
                    }
                }

                await context.SaveChangesAsync();
                count += batch.Length;
                progress?.Report(count);
            }

            context.ChangeTracker.AutoDetectChangesEnabled = true;
            await transaction.CommitAsync();

            return (data.Professions.Count, totalSiz);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ExportToJsonAsync(string outputPath)
    {
        using var context = new SizDbContext();

        var professions = await context.Professions
            .Include(p => p.SizList)
            .OrderBy(p => p.Id)
            .ToListAsync();

        var data = new SizDatabase
        {
            Metadata = new SizMetadata
            {
                Version = "1.0",
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                Source = "Экспорт из СИЗ Менеджер",
                Description = "Единые типовые нормы выдачи СИЗ",
                TotalProfessions = professions.Count
            },
            Professions = professions.Select(p => new JsonProfession
            {
                Number = p.Number,
                Name = p.Name,
                SizList = p.SizList.Select(s => new JsonSizItem
                {
                    Type = s.Type,
                    Name = s.Name,
                    Norm = s.Norm
                }).ToList()
            }).ToList()
        };

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        await File.WriteAllTextAsync(outputPath, json);
    }

    private static void ValidateData(SizDatabase data)
    {
        if (data.Professions == null || data.Professions.Count == 0)
            throw new InvalidOperationException("JSON не содержит профессий");

        foreach (var prof in data.Professions)
        {
            if (string.IsNullOrWhiteSpace(prof.Name))
                throw new InvalidOperationException($"Профессия №{prof.Number} не имеет названия");
            if (prof.SizList == null)
                throw new InvalidOperationException($"Профессия \"{prof.Name}\" не имеет списка СИЗ");
        }
    }
}
