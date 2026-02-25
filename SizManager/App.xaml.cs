using System.Windows;
using SizManager.Helpers;
using SizManager.Services;
using SizManager.Services.Database;
using SizManager.Services.Export;
using SizManager.Services.Import;
using SizManager.ViewModels;
using SizManager.Views;

namespace SizManager;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure QuestPDF license
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        // Ensure directories exist
        AppPaths.EnsureDirectories();

        // Ensure database exists
        try
        {
            using var ctx = new SizDbContext();
            ctx.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Database initialization");
            MessageBox.Show(
                $"Ошибка инициализации базы данных:\n{ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        // Create services
        var dialogService = new DialogService();
        var backupService = new BackupService();
        var importService = new JsonImportService(backupService);

        // Auto-import embedded database on first launch (if no professions exist)
        try
        {
            using var checkCtx = new SizDbContext();
            if (!checkCtx.Professions.Any())
            {
                var task = importService.ImportFromEmbeddedResourceAsync();
                task.GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Auto-import embedded database");
            // Non-fatal: user can import manually later
        }
        var validationService = new ValidationService();
        var docxExportService = new DocxExportService();
        var pdfExportService = new PdfExportService();
        var excelExportService = new ExcelExportService();

        // Create and show main window
        var mainVM = new MainViewModel(
            importService, backupService, validationService,
            docxExportService, pdfExportService, excelExportService,
            dialogService);

        var mainWindow = new MainWindow { DataContext = mainVM };
        MainWindow = mainWindow;
        mainWindow.Show();
    }
}
