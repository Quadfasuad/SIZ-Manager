using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SizManager.Helpers;
using SizManager.Services.Export;

namespace SizManager.ViewModels;

public partial class DermatologicalProtectionViewModel : ObservableObject
{
    private readonly DialogService _dialogService;
    private readonly DermatologicalExcelExportService _excelExportService;
    private readonly DermatologicalPdfExportService _pdfExportService;

    [ObservableProperty] private ObservableCollection<DermatologicalProtectionRow> _rows = new();
    [ObservableProperty] private ObservableCollection<DermatologicalRequirementRow> _requirements = new();
    [ObservableProperty] private DermatologicalProtectionRow? _selectedRow;

    public DermatologicalProtectionViewModel(
        DialogService dialogService,
        DermatologicalExcelExportService excelExportService,
        DermatologicalPdfExportService pdfExportService)
    {
        _dialogService = dialogService;
        _excelExportService = excelExportService;
        _pdfExportService = pdfExportService;
        ResetDefaults();
    }

    public int SelectedCount => Rows.Count(r => r.IsIncluded);
    public int RequirementCount => Requirements.Count(r => !r.IsHeader);

    partial void OnRowsChanged(ObservableCollection<DermatologicalProtectionRow> value)
    {
        foreach (var row in value)
        {
            AttachRow(row);
        }
        OnPropertyChanged(nameof(SelectedCount));
        RebuildRequirements();
    }

    private void AttachRow(DermatologicalProtectionRow row)
    {
        row.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DermatologicalProtectionRow.IsIncluded))
            {
                OnPropertyChanged(nameof(SelectedCount));
                RebuildRequirements();
            }
        };
    }

    [RelayCommand]
    private void AddRow()
    {
        var row = new DermatologicalProtectionRow
        {
            IsIncluded = true,
            WorkType = "Новый вид работ"
        };
        AttachRow(row);
        Rows.Add(row);
        OnPropertyChanged(nameof(SelectedCount));
        RebuildRequirements();
    }

    [RelayCommand]
    private void RemoveSelected()
    {
        if (SelectedRow == null)
            return;

        Rows.Remove(SelectedRow);
        OnPropertyChanged(nameof(SelectedCount));
        RebuildRequirements();
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var row in Rows)
            row.IsIncluded = true;

        OnPropertyChanged(nameof(SelectedCount));
        RebuildRequirements();
    }

    [RelayCommand]
    private void ClearSelection()
    {
        foreach (var row in Rows)
            row.IsIncluded = false;

        OnPropertyChanged(nameof(SelectedCount));
        RebuildRequirements();
    }

    [RelayCommand]
    private void ResetDefaults()
    {
        Rows = new ObservableCollection<DermatologicalProtectionRow>(CreateDefaultRows());
        RebuildRequirements();
    }

    [RelayCommand]
    private void ExportExcel()
    {
        if (Requirements.Count == 0)
        {
            _dialogService.ShowWarning("Выберите хотя бы один вид работ для экспорта");
            return;
        }

        var path = _dialogService.SaveFileDialog(
            "Excel файл (*.xlsx)|*.xlsx",
            "Дерматологические_СИЗ.xlsx",
            "Сохранить таблицу дерматологических СИЗ");

        if (path == null)
            return;

        try
        {
            _excelExportService.Export(Requirements, path);
            _dialogService.ShowMessage($"Файл сохранен:\n{path}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Export dermatological SIZ");
            _dialogService.ShowError($"Ошибка экспорта:\n{ex.Message}");
        }
    }

    [RelayCommand]
    private void ExportPdf()
    {
        if (Requirements.Count == 0)
        {
            _dialogService.ShowWarning("Выберите хотя бы один вид работ для экспорта");
            return;
        }

        var path = _dialogService.SaveFileDialog(
            "PDF документ (*.pdf)|*.pdf",
            "Дерматологические_СИЗ.pdf",
            "Сохранить таблицу дерматологических СИЗ");

        if (path == null)
            return;

        try
        {
            _pdfExportService.Export(Requirements, path);
            _dialogService.ShowMessage($"Файл сохранен:\n{path}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Export dermatological SIZ PDF");
            _dialogService.ShowError($"Ошибка экспорта:\n{ex.Message}");
        }
    }

    private void RebuildRequirements()
    {
        var rows = new List<DermatologicalRequirementRow>();

        foreach (var row in Rows.Where(r => r.IsIncluded))
        {
            var startCount = rows.Count;
            rows.Add(new DermatologicalRequirementRow
            {
                IsHeader = true,
                ProductType = row.WorkType
            });

            AddRequirement(rows, "Средства для защиты от биологических факторов: бактерий", row.Antibacterial);
            AddRequirement(rows, "Средства для защиты от биологических факторов: грибов", row.Antifungal);
            AddRequirement(rows, "Средства для защиты от биологических факторов: вирусов", row.Antiviral);
            AddRequirement(rows, "Средства гидрофобного действия", row.Hydrophobic);
            AddRequirement(rows, "Средства для защиты от низких температур и ветра", row.ColdAndWind);
            AddRequirement(rows, "Средства для защиты от УФ-излучения диапазонов A, B, C", row.UvProtection);
            AddRequirement(rows, "Репеллентные средства", row.Repellent);
            AddRequirement(rows, "Инсектоакарицидные средства", row.Insectoacaricidal);
            AddRequirement(rows, "Средства для очищения от неустойчивых загрязнений", row.Cleansing);
            AddRequirement(rows, "Средства регенерирующего (восстанавливающего) типа", row.Regenerating);

            if (rows.Count == startCount + 1)
                rows.RemoveAt(startCount);
        }

        Requirements = new ObservableCollection<DermatologicalRequirementRow>(rows);
        OnPropertyChanged(nameof(RequirementCount));
    }

    private static void AddRequirement(ICollection<DermatologicalRequirementRow> rows, string productType, string norm)
    {
        if (string.IsNullOrWhiteSpace(norm))
            return;

        rows.Add(new DermatologicalRequirementRow
        {
            ProductType = productType,
            Norm = norm
        });
    }

    private static IEnumerable<DermatologicalProtectionRow> CreateDefaultRows()
    {
        return new[]
        {
            new DermatologicalProtectionRow
            {
                WorkType = "При производстве продуктов питания, контакте с продуктами питания на предприятиях общественного питания и другие",
                Antibacterial = "100",
                Cleansing = "250/200",
                Regenerating = "100"
            },
            new DermatologicalProtectionRow
            {
                WorkType = "При выполнении работ в средствах защиты ног (закрытая специальная обувь)",
                Antifungal = "100",
                Cleansing = "250/200",
                Regenerating = "100"
            },
            new DermatologicalProtectionRow
            {
                WorkType = "При работах, выполняемых в резиновых перчатках или перчатках из полимерных материалов (без натуральной подкладки)",
                Hydrophobic = "100",
                Regenerating = "100"
            },
            new DermatologicalProtectionRow
            {
                WorkType = "При выполнении работ удаленно от санитарно-бытовых узлов",
                Antibacterial = "100"
            },
            new DermatologicalProtectionRow
            {
                WorkType = "При производстве медицинских препаратов и их составляющих, всему медицинскому персоналу (врачам, медсестрам, акушерам и т.д.)",
                Antiviral = "100",
                Cleansing = "250/200",
                Regenerating = "100"
            },
            new DermatologicalProtectionRow
            {
                WorkType = "При выполнении работ при воздействии пониженных температур воздуха, ветра",
                ColdAndWind = "100",
                Cleansing = "250/200",
                Regenerating = "100"
            },
            new DermatologicalProtectionRow
            {
                WorkType = "При выполнении наружных и иных работ, связанных с воздействием УФ-излучения диапазонов A, B, C, при проведении сварочных работ",
                UvProtection = "100",
                Cleansing = "250/200",
                Regenerating = "100"
            },
            new DermatologicalProtectionRow
            {
                WorkType = "При выполнении работ в районах, где сезонно наблюдается массовый лет кровососущих насекомых",
                Repellent = "200",
                Cleansing = "250/200",
                Regenerating = "100"
            },
            new DermatologicalProtectionRow
            {
                WorkType = "При выполнении работ в районах, где наблюдается распространение и активность паукообразных (иксодовые клещи и другие), с учетом сезонной специфики региона",
                Insectoacaricidal = "200",
                Cleansing = "250/200",
                Regenerating = "100"
            },
            new DermatologicalProtectionRow
            {
                WorkType = "В профилактических целях для проведения дезинфекционных мероприятий в период распространения вирусной инфекции (заболеваний)",
                Antiviral = "100",
                Cleansing = "250/200",
                Regenerating = "100"
            }
        };
    }
}

public partial class DermatologicalProtectionRow : ObservableObject
{
    [ObservableProperty] private bool _isIncluded;
    [ObservableProperty] private string _workType = string.Empty;
    [ObservableProperty] private string _antibacterial = string.Empty;
    [ObservableProperty] private string _antifungal = string.Empty;
    [ObservableProperty] private string _antiviral = string.Empty;
    [ObservableProperty] private string _hydrophobic = string.Empty;
    [ObservableProperty] private string _coldAndWind = string.Empty;
    [ObservableProperty] private string _uvProtection = string.Empty;
    [ObservableProperty] private string _repellent = string.Empty;
    [ObservableProperty] private string _insectoacaricidal = string.Empty;
    [ObservableProperty] private string _cleansing = string.Empty;
    [ObservableProperty] private string _regenerating = string.Empty;
}

public class DermatologicalRequirementRow
{
    public bool IsHeader { get; set; }
    public string ProductType { get; set; } = string.Empty;
    public string Norm { get; set; } = string.Empty;
}
