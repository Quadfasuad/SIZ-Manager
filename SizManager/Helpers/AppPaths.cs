using System.IO;

namespace SizManager.Helpers;

public static class AppPaths
{
    public static string BaseDir => AppDomain.CurrentDomain.BaseDirectory;
    public static string DatabasePath => Path.Combine(BaseDir, "database.db");
    public static string TemplatesDir => Path.Combine(BaseDir, "Templates");
    public static string BackupsDir => Path.Combine(BaseDir, "Backups");
    public static string ErrorLogPath => Path.Combine(BaseDir, "errors.log");

    public static string TemplatePath =>
        Path.Combine(TemplatesDir, "card_template_with_placeholders.docx");

    public static void EnsureDirectories()
    {
        Directory.CreateDirectory(TemplatesDir);
        Directory.CreateDirectory(BackupsDir);
    }
}
