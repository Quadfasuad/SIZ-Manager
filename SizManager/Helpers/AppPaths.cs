using System.IO;

namespace SizManager.Helpers;

public static class AppPaths
{
    /// <summary>Папка установки приложения (только чтение).</summary>
    public static string AppDir => AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>Папка пользовательских данных (%APPDATA%/SizManager/).</summary>
    public static string DataDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SizManager");

    // --- Пользовательские данные (DataDir, запись разрешена) ---
    public static string DatabasePath => Path.Combine(DataDir, "database.db");
    public static string BackupsDir => Path.Combine(DataDir, "Backups");
    public static string UpdatesDir => Path.Combine(DataDir, "Updates");
    public static string ErrorLogPath => Path.Combine(DataDir, "errors.log");

    // --- Файлы приложения (AppDir, только чтение) ---
    public static string TemplatesDir => Path.Combine(AppDir, "Templates");
    public static string TemplatePath =>
        Path.Combine(TemplatesDir, "card_template_with_placeholders.docx");

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(DataDir);
        Directory.CreateDirectory(BackupsDir);
    }
}
