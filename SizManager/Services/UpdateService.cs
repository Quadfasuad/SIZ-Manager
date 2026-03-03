using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using SizManager.Helpers;
using SizManager.Models;

namespace SizManager.Services;

public class UpdateService
{
    private const string GitHubApiUrl =
        "https://api.github.com/repos/Quadfasuad/SIZ-Manager/releases/latest";

    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    static UpdateService()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SizManager-Updater");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
    }

    /// <summary>
    /// Проверяет наличие обновлений на GitHub Releases.
    /// Возвращает UpdateInfo если доступна новая версия, иначе null.
    /// </summary>
    public async Task<UpdateInfo?> CheckForUpdateAsync()
    {
        var response = await _httpClient.GetAsync(GitHubApiUrl);

        // 404 = релизов ещё нет, не ошибка
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = doc.RootElement;

        var tagName = root.GetProperty("tag_name").GetString();
        if (string.IsNullOrEmpty(tagName)) return null;

        // Убираем prefix "v" если есть: "v1.1.0" → "1.1.0"
        var versionString = tagName.TrimStart('v', 'V');
        if (!Version.TryParse(versionString, out var remoteVersion)) return null;

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        if (currentVersion == null || remoteVersion <= currentVersion) return null;

        var changelog = root.GetProperty("body").GetString() ?? "";

        // Ищем .exe в assets
        string? downloadUrl = null;
        string? fileName = null;

        if (root.TryGetProperty("assets", out var assets))
        {
            foreach (var asset in assets.EnumerateArray())
            {
                var name = asset.GetProperty("name").GetString() ?? "";
                if (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    downloadUrl = asset.GetProperty("browser_download_url").GetString();
                    fileName = name;
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(downloadUrl) || string.IsNullOrEmpty(fileName))
            return null;

        return new UpdateInfo(versionString, changelog, downloadUrl, fileName);
    }

    /// <summary>
    /// Скачивает инсталлятор обновления в папку Updates.
    /// Возвращает путь к скачанному файлу.
    /// </summary>
    public async Task<string> DownloadUpdateAsync(UpdateInfo info, IProgress<int>? progress = null)
    {
        var filePath = Path.Combine(AppPaths.UpdatesDir, info.FileName);

        // Удаляем старые файлы обновлений
        if (Directory.Exists(AppPaths.UpdatesDir))
        {
            foreach (var file in Directory.GetFiles(AppPaths.UpdatesDir, "*.exe"))
            {
                try { File.Delete(file); } catch { }
            }
        }
        Directory.CreateDirectory(AppPaths.UpdatesDir);

        using var response = await _httpClient.GetAsync(info.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        long downloadedBytes = 0;

        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            downloadedBytes += bytesRead;

            if (totalBytes > 0)
            {
                progress?.Report((int)(downloadedBytes * 100 / totalBytes));
            }
        }

        progress?.Report(100);
        return filePath;
    }

    /// <summary>Возвращает текущую версию приложения.</summary>
    public static string GetCurrentVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
    }
}
