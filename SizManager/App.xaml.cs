using System.Windows;
using System.Windows.Threading;
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

        var splashWindow = new SplashWindow();
        splashWindow.Show();
        Dispatcher.Invoke(() => { }, DispatcherPriority.Render);

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
            splashWindow.Close();
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

        // Auto-import embedded database on first launch or when the bundled reference is newer.
        try
        {
            using var checkCtx = new SizDbContext();
            var currentProfessionCount = checkCtx.Professions.Count();
            var embeddedProfessionCount = importService.GetEmbeddedProfessionCountAsync()
                .GetAwaiter()
                .GetResult();

            if (currentProfessionCount < embeddedProfessionCount)
            {
                if (currentProfessionCount > 0)
                {
                    backupService.CreateBackupAsync().GetAwaiter().GetResult();
                }

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
        var dermatologicalExcelExportService = new DermatologicalExcelExportService();
        var dermatologicalPdfExportService = new DermatologicalPdfExportService();
        var updateService = new UpdateService();

        // Create and show main window
        var mainVM = new MainViewModel(
            importService, backupService, validationService,
            docxExportService, pdfExportService, excelExportService,
            dermatologicalExcelExportService, dermatologicalPdfExportService,
            updateService, dialogService);

        var mainWindow = new MainWindow { DataContext = mainVM };
        MainWindow = mainWindow;
        mainWindow.Show();
        splashWindow.Close();

        // Background update check (non-blocking)
        _ = CheckForUpdateAsync(updateService);
    }

    private async Task CheckForUpdateAsync(UpdateService updateService)
    {
        try
        {
            var update = await updateService.CheckForUpdateAsync();
            if (update != null)
            {
                var window = new UpdateWindow(update, updateService);
                window.Owner = MainWindow;
                window.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Update check");
        }
    }
}
