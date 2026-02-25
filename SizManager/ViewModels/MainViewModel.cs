using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SizManager.Helpers;
using SizManager.Models;
using SizManager.Services;
using SizManager.Services.Database;
using SizManager.Services.Export;
using SizManager.Services.Import;
using SizManager.Views;

namespace SizManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly JsonImportService _importService;
    private readonly BackupService _backupService;
    private readonly DialogService _dialogService;

    // Database info
    [ObservableProperty] private int _professionCount;
    [ObservableProperty] private string _databaseVersion = "—";
    [ObservableProperty] private string _lastUpdate = "—";
    [ObservableProperty] private bool _isImporting;
    [ObservableProperty] private int _importProgress;
    [ObservableProperty] private string _statusBarText = "Готово";

    // Child ViewModel
    public EmployeeCardViewModel Card { get; }

    public MainViewModel(
        JsonImportService importService,
        BackupService backupService,
        ValidationService validationService,
        DocxExportService docxExportService,
        PdfExportService pdfExportService,
        ExcelExportService excelExportService,
        DialogService dialogService)
    {
        _importService = importService;
        _backupService = backupService;
        _dialogService = dialogService;

        Card = new EmployeeCardViewModel(
            validationService, docxExportService, pdfExportService,
            excelExportService, dialogService);

        RefreshDatabaseInfo();
    }

    private void RefreshDatabaseInfo()
    {
        try
        {
            using var context = new SizDbContext();
            ProfessionCount = context.Professions.Count();
            if (ProfessionCount > 0)
            {
                DatabaseVersion = "1.0";
                LastUpdate = context.Professions.Max(p => p.UpdatedAt).ToString("dd.MM.yyyy");
            }
            else
            {
                DatabaseVersion = "—";
                LastUpdate = "—";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "RefreshDatabaseInfo");
        }
    }

    [RelayCommand]
    private async Task ImportJsonAsync()
    {
        var path = _dialogService.OpenFileDialog("JSON файлы (*.json)|*.json", "Импорт справочника из JSON");
        if (path == null) return;

        if (ProfessionCount > 0)
        {
            if (!_dialogService.ShowConfirmation(
                "Текущий справочник будет заменен.\nРезервная копия будет создана автоматически.\n\nПродолжить?"))
                return;
        }

        IsImporting = true;
        ImportProgress = 0;
        StatusBarText = "Импорт справочника...";

        try
        {
            var (professions, sizItems) = await _importService.ImportAsync(path, new Progress<int>(count =>
            {
                ImportProgress = count;
                StatusBarText = $"Импорт: {count} профессий...";
            }));

            RefreshDatabaseInfo();
            Card.RefreshProfessions();
            StatusBarText = $"Импортировано профессий: {professions}, СИЗ: {sizItems}";
            _dialogService.ShowMessage($"Импорт завершен!\n\nПрофессий: {professions}\nСИЗ: {sizItems}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ImportJson");
            StatusBarText = "Ошибка импорта";
            _dialogService.ShowError($"Ошибка импорта:\n{ex.Message}");
        }
        finally
        {
            IsImporting = false;
        }
    }

    [RelayCommand]
    private void OpenEmployeeList()
    {
        var window = new EmployeeListWindow();
        var vm = new EmployeeListViewModel(_dialogService);
        window.DataContext = vm;
        window.Owner = System.Windows.Application.Current.MainWindow;

        if (window.ShowDialog() == true && vm.SelectedEmployeeId > 0)
        {
            Card.LoadEmployee(vm.SelectedEmployeeId);
        }
    }

    [RelayCommand]
    private void OpenProfessionList()
    {
        var window = new ProfessionListWindow();
        var vm = new ProfessionListViewModel();
        window.DataContext = vm;
        window.Owner = System.Windows.Application.Current.MainWindow;
        window.ShowDialog();
    }

    [RelayCommand]
    private void ShowAbout()
    {
        var window = new AboutWindow();
        window.Owner = System.Windows.Application.Current.MainWindow;
        window.ShowDialog();
    }

    [RelayCommand]
    private async Task ManageBackupsAsync()
    {
        try
        {
            var backups = await _backupService.GetBackupsAsync();
            if (backups.Count == 0)
            {
                _dialogService.ShowMessage("Резервные копии отсутствуют");
                return;
            }

            var list = string.Join("\n", backups.Select((b, i) =>
                $"{i + 1}. {System.IO.Path.GetFileName(b)}"));

            if (_dialogService.ShowConfirmation(
                $"Доступные резервные копии:\n\n{list}\n\nВосстановить последнюю копию?"))
            {
                await _backupService.RestoreBackupAsync(backups[0]);
                RefreshDatabaseInfo();
                Card.RefreshProfessions();
                StatusBarText = "База данных восстановлена из резервной копии";
                _dialogService.ShowMessage("База данных восстановлена");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ManageBackups");
            _dialogService.ShowError($"Ошибка: {ex.Message}");
        }
    }

    [RelayCommand]
    private void NewCard()
    {
        Card.NewCardCommand.Execute(null);
        StatusBarText = "Новая карточка";
    }
}
