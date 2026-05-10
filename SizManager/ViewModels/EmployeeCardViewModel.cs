using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SizManager.Helpers;
using SizManager.Models;
using SizManager.Services;
using SizManager.Services.Database;
using SizManager.Services.Export;

namespace SizManager.ViewModels;

public partial class EmployeeCardViewModel : ObservableObject
{
    private readonly ValidationService _validationService;
    private readonly DocxExportService _docxExportService;
    private readonly PdfExportService _pdfExportService;
    private readonly ExcelExportService _excelExportService;
    private readonly DialogService _dialogService;

    private int _employeeId;
    private List<Profession> _allProfessions = new();
    private bool _isLoadingEmployee;
    private bool _isSelectingProfession; // prevents filtering when we set text from selection
    private EmployeeIdentitySnapshot? _loadedIdentity;

    // Form fields
    [ObservableProperty] private string _cardNumber = string.Empty;
    [ObservableProperty] private string _lastName = string.Empty;
    [ObservableProperty] private string _firstName = string.Empty;
    [ObservableProperty] private string _middleName = string.Empty;
    [ObservableProperty] private string _gender = string.Empty;
    [ObservableProperty] private string _personnelNumber = string.Empty;
    [ObservableProperty] private string _department = string.Empty;
    [ObservableProperty] private string _professionSearchText = string.Empty;
    [ObservableProperty] private Profession? _selectedProfession;
    [ObservableProperty] private DateTime? _hireDate;
    [ObservableProperty] private DateTime? _changeDate;
    [ObservableProperty] private string _heightText = string.Empty;
    [ObservableProperty] private string _clothingSize = string.Empty;
    [ObservableProperty] private string _shoeSize = string.Empty;
    [ObservableProperty] private string _headwearSize = string.Empty;
    [ObservableProperty] private string _respiratorsSize = string.Empty;
    [ObservableProperty] private string _glovesSize = string.Empty;

    // SIZ items
    [ObservableProperty] private ObservableCollection<EmployeeSizRow> _sizItems = new();
    [ObservableProperty] private EmployeeSizRow? _selectedSizItem;

    // Profession autocomplete
    [ObservableProperty] private ObservableCollection<Profession> _filteredProfessions = new();
    [ObservableProperty] private bool _isDropDownOpen;

    // Status
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isDirty;
    [ObservableProperty] private bool _isBusy;

    public EmployeeCardViewModel(
        ValidationService validationService,
        DocxExportService docxExportService,
        PdfExportService pdfExportService,
        ExcelExportService excelExportService,
        DialogService dialogService)
    {
        _validationService = validationService;
        _docxExportService = docxExportService;
        _pdfExportService = pdfExportService;
        _excelExportService = excelExportService;
        _dialogService = dialogService;

        LoadProfessions();
    }

    public bool IsExistingCard => _employeeId > 0;

    private void LoadProfessions()
    {
        try
        {
            using var context = new SizDbContext();
            _allProfessions = context.Professions
                .OrderBy(p => p.Name)
                .ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "LoadProfessions");
        }
    }

    public void RefreshProfessions()
    {
        LoadProfessions();
    }

    partial void OnProfessionSearchTextChanged(string value)
    {
        // Don't filter when we're programmatically setting text after selection
        if (_isSelectingProfession || _isLoadingEmployee)
            return;
        FilterProfessions(value);
    }

    partial void OnSelectedProfessionChanged(Profession? value)
    {
        if (value != null && !_isLoadingEmployee)
        {
            // Set the text to show the selected profession name
            _isSelectingProfession = true;
            ProfessionSearchText = value.DisplayName;
            IsDropDownOpen = false;
            _isSelectingProfession = false;

            LoadSizForProfession(value);
            IsDirty = true;
        }
    }

    private void FilterProfessions(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText) || searchText.Length < 1)
        {
            FilteredProfessions.Clear();
            IsDropDownOpen = false;
            return;
        }

        // Search by both name AND number
        var filtered = _allProfessions
            .Where(p => p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                     || p.Number.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .Take(50)
            .ToList();

        FilteredProfessions = new ObservableCollection<Profession>(filtered);
        IsDropDownOpen = filtered.Count > 0;
    }

    private void LoadSizForProfession(Profession profession)
    {
        try
        {
            using var context = new SizDbContext();
            var sizList = context.ProfessionSIZ
                .Where(s => s.ProfessionId == profession.Id)
                .ToList();

            SizItems.Clear();
            foreach (var siz in sizList)
            {
                SizItems.Add(new EmployeeSizRow
                {
                    Type = siz.Type,
                    Name = siz.Name,
                    Norm = siz.Norm
                });
            }

            StatusMessage = $"Загружено СИЗ: {sizList.Count}";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "LoadSizForProfession");
            _dialogService.ShowError($"Ошибка загрузки СИЗ: {ex.Message}");
        }
    }

    [RelayCommand]
    private void NewCard()
    {
        if (IsDirty)
        {
            if (!_dialogService.ShowConfirmation("Текущая карточка не сохранена. Создать новую?"))
                return;
        }

        _isSelectingProfession = true;
        _employeeId = 0;
        CardNumber = string.Empty;
        LastName = string.Empty;
        FirstName = string.Empty;
        MiddleName = string.Empty;
        Gender = string.Empty;
        PersonnelNumber = string.Empty;
        Department = string.Empty;
        ProfessionSearchText = string.Empty;
        SelectedProfession = null;
        HireDate = null;
        ChangeDate = null;
        HeightText = string.Empty;
        ClothingSize = string.Empty;
        ShoeSize = string.Empty;
        HeadwearSize = string.Empty;
        RespiratorsSize = string.Empty;
        GlovesSize = string.Empty;
        SizItems.Clear();
        _loadedIdentity = null;
        IsDirty = false;
        StatusMessage = "Новая карточка";
        OnPropertyChanged(nameof(IsExistingCard));
        _isSelectingProfession = false;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        var employee = BuildEmployee();

        var errors = _validationService.ValidateEmployee(employee);
        if (errors.Count > 0)
        {
            _dialogService.ShowWarning(string.Join("\n", errors), "Ошибки валидации");
            return;
        }

        IsBusy = true;
        try
        {
            using var context = new SizDbContext();
            var saveAsNew = false;

            if (_employeeId > 0)
            {
                if (HasEmployeeIdentityChanged(employee) &&
                    _dialogService.ShowConfirmation(
                        "Изменены основные данные сотрудника.\n\nСохранить карточку как новую запись?\n\nДа — создать новую карточку.\nНет — обновить текущую карточку."))
                {
                    saveAsNew = true;
                }
            }

            if (_employeeId > 0 && !saveAsNew)
            {
                // Update existing
                var existing = await context.Employees
                    .Include(e => e.SizList)
                    .FirstOrDefaultAsync(e => e.Id == _employeeId);

                if (existing == null)
                {
                    _dialogService.ShowError("Карточка не найдена в базе данных");
                    return;
                }

                UpdateEmployee(existing, employee);
                existing.UpdatedAt = DateTime.Now;

                // Replace SIZ items
                context.EmployeeSIZ.RemoveRange(existing.SizList);
                foreach (var siz in SizItems)
                {
                    context.EmployeeSIZ.Add(new EmployeeSIZ
                    {
                        EmployeeId = _employeeId,
                        Type = siz.Type,
                        Name = siz.Name,
                        Norm = siz.Norm
                    });
                }
            }
            else
            {
                // Insert new
                employee.Id = 0;
                employee.CreatedAt = DateTime.Now;
                employee.UpdatedAt = DateTime.Now;
                context.Employees.Add(employee);
                await context.SaveChangesAsync();

                _employeeId = employee.Id;

                foreach (var siz in SizItems)
                {
                    context.EmployeeSIZ.Add(new EmployeeSIZ
                    {
                        EmployeeId = _employeeId,
                        Type = siz.Type,
                        Name = siz.Name,
                        Norm = siz.Norm
                    });
                }
            }

            await context.SaveChangesAsync();

            IsDirty = false;
            _loadedIdentity = EmployeeIdentitySnapshot.From(employee);
            OnPropertyChanged(nameof(IsExistingCard));
            StatusMessage = "Карточка сохранена";
            _dialogService.ShowMessage("Карточка успешно сохранена");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "SaveEmployee");
            _dialogService.ShowError($"Ошибка сохранения: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void AddSiz()
    {
        SizItems.Add(new EmployeeSizRow
        {
            Type = "Новый тип",
            Name = "Новое наименование",
            Norm = "1 шт."
        });
        IsDirty = true;
    }

    [RelayCommand]
    private void RemoveSiz()
    {
        if (SelectedSizItem == null) return;
        SizItems.Remove(SelectedSizItem);
        IsDirty = true;
    }

    [RelayCommand]
    private void ExportDocx()
    {
        var name = BuildExportFileName();
        ExportInternal("Word документ (*.docx)|*.docx",
            $"Карточка_{name}.docx",
            (emp, siz, path) => _docxExportService.Export(emp, siz, path));
    }

    [RelayCommand]
    private void ExportPdf()
    {
        var name = BuildExportFileName();
        ExportInternal("PDF документ (*.pdf)|*.pdf",
            $"Карточка_{name}.pdf",
            (emp, siz, path) => _pdfExportService.Export(emp, siz, path));
    }

    [RelayCommand]
    private void ExportExcel()
    {
        var name = BuildExportFileName();
        ExportInternal("Excel файл (*.xlsx)|*.xlsx",
            $"Карточка_{name}.xlsx",
            (emp, siz, path) => _excelExportService.Export(emp, siz, path));
    }

    private string BuildExportFileName()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(LastName)) parts.Add(LastName.Trim());
        if (!string.IsNullOrWhiteSpace(FirstName)) parts.Add(FirstName.Trim());
        if (parts.Count == 0 && SelectedProfession != null)
            parts.Add(SelectedProfession.Name);
        if (parts.Count == 0) parts.Add("Новая");
        return string.Join("_", parts);
    }

    private void ExportInternal(string filter, string defaultName, Action<Employee, ICollection<EmployeeSIZ>, string> exportAction)
    {
        if (string.IsNullOrWhiteSpace(ProfessionSearchText))
        {
            _dialogService.ShowWarning("Выберите профессию для экспорта");
            return;
        }

        var path = _dialogService.SaveFileDialog(filter, defaultName);
        if (path == null) return;

        try
        {
            var employee = BuildEmployee();
            var sizList = SizItems.Select(s => new EmployeeSIZ
            {
                Type = s.Type,
                Name = s.Name,
                Norm = s.Norm
            }).ToList();

            exportAction(employee, sizList, path);
            StatusMessage = $"Экспорт выполнен: {System.IO.Path.GetFileName(path)}";
            _dialogService.ShowMessage($"Файл сохранен:\n{path}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Export");
            _dialogService.ShowError($"Ошибка экспорта: {ex.Message}");
        }
    }

    public void LoadEmployee(int employeeId)
    {
        _isLoadingEmployee = true;
        try
        {
            using var context = new SizDbContext();
            var employee = context.Employees
                .Include(e => e.SizList)
                .FirstOrDefault(e => e.Id == employeeId);

            if (employee == null)
            {
                _dialogService.ShowError("Карточка не найдена");
                return;
            }

            _employeeId = employee.Id;
            CardNumber = employee.CardNumber ?? "";
            LastName = employee.LastName;
            FirstName = employee.FirstName;
            MiddleName = employee.MiddleName ?? "";
            Gender = employee.Gender ?? "";
            PersonnelNumber = employee.PersonnelNumber ?? "";
            Department = employee.Department ?? "";

            // Set profession
            var profession = _allProfessions.FirstOrDefault(p => p.Id == employee.ProfessionId);
            if (profession != null)
            {
                ProfessionSearchText = profession.DisplayName;
                SelectedProfession = profession;
            }
            else
            {
                ProfessionSearchText = employee.ProfessionName;
            }

            HireDate = employee.HireDate;
            ChangeDate = employee.ChangeDate;
            HeightText = employee.Height?.ToString() ?? "";
            ClothingSize = employee.ClothingSize ?? "";
            ShoeSize = employee.ShoeSize ?? "";
            HeadwearSize = employee.HeadwearSize ?? "";
            RespiratorsSize = employee.RespiratorsSize ?? "";
            GlovesSize = employee.GlovesSize ?? "";

            SizItems.Clear();
            foreach (var siz in employee.SizList)
            {
                SizItems.Add(new EmployeeSizRow
                {
                    Type = siz.Type,
                    Name = siz.Name,
                    Norm = siz.Norm
                });
            }

            IsDirty = false;
            _loadedIdentity = EmployeeIdentitySnapshot.From(employee);
            OnPropertyChanged(nameof(IsExistingCard));
            StatusMessage = $"Карточка: {employee.FullName}";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "LoadEmployee");
            _dialogService.ShowError($"Ошибка загрузки карточки: {ex.Message}");
        }
        finally
        {
            _isLoadingEmployee = false;
        }
    }

    private Employee BuildEmployee()
    {
        int.TryParse(HeightText, out var height);

        return new Employee
        {
            Id = _employeeId,
            CardNumber = string.IsNullOrWhiteSpace(CardNumber) ? null : CardNumber,
            LastName = LastName.Trim(),
            FirstName = FirstName.Trim(),
            MiddleName = string.IsNullOrWhiteSpace(MiddleName) ? null : MiddleName.Trim(),
            Gender = string.IsNullOrWhiteSpace(Gender) ? null : Gender,
            PersonnelNumber = string.IsNullOrWhiteSpace(PersonnelNumber) ? null : PersonnelNumber.Trim(),
            Department = string.IsNullOrWhiteSpace(Department) ? null : Department.Trim(),
            ProfessionId = SelectedProfession?.Id,
            ProfessionName = SelectedProfession?.Name ?? ProfessionSearchText.Trim(),
            HireDate = HireDate,
            ChangeDate = ChangeDate,
            Height = height > 0 ? height : null,
            ClothingSize = string.IsNullOrWhiteSpace(ClothingSize) ? null : ClothingSize.Trim(),
            ShoeSize = string.IsNullOrWhiteSpace(ShoeSize) ? null : ShoeSize.Trim(),
            HeadwearSize = string.IsNullOrWhiteSpace(HeadwearSize) ? null : HeadwearSize.Trim(),
            RespiratorsSize = string.IsNullOrWhiteSpace(RespiratorsSize) ? null : RespiratorsSize.Trim(),
            GlovesSize = string.IsNullOrWhiteSpace(GlovesSize) ? null : GlovesSize.Trim(),
        };
    }

    private static void UpdateEmployee(Employee target, Employee source)
    {
        target.CardNumber = source.CardNumber;
        target.LastName = source.LastName;
        target.FirstName = source.FirstName;
        target.MiddleName = source.MiddleName;
        target.Gender = source.Gender;
        target.PersonnelNumber = source.PersonnelNumber;
        target.Department = source.Department;
        target.ProfessionId = source.ProfessionId;
        target.ProfessionName = source.ProfessionName;
        target.HireDate = source.HireDate;
        target.ChangeDate = source.ChangeDate;
        target.Height = source.Height;
        target.ClothingSize = source.ClothingSize;
        target.ShoeSize = source.ShoeSize;
        target.HeadwearSize = source.HeadwearSize;
        target.RespiratorsSize = source.RespiratorsSize;
        target.GlovesSize = source.GlovesSize;
    }

    public void MarkDirty()
    {
        IsDirty = true;
    }

    private bool HasEmployeeIdentityChanged(Employee current)
    {
        if (_loadedIdentity == null)
            return false;

        return !_loadedIdentity.Equals(EmployeeIdentitySnapshot.From(current));
    }

    private sealed class EmployeeIdentitySnapshot : IEquatable<EmployeeIdentitySnapshot>
    {
        public string LastName { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string MiddleName { get; init; } = string.Empty;
        public string PersonnelNumber { get; init; } = string.Empty;
        public string Department { get; init; } = string.Empty;
        public int? ProfessionId { get; init; }
        public string ProfessionName { get; init; } = string.Empty;

        public static EmployeeIdentitySnapshot From(Employee employee)
        {
            return new EmployeeIdentitySnapshot
            {
                LastName = Normalize(employee.LastName),
                FirstName = Normalize(employee.FirstName),
                MiddleName = Normalize(employee.MiddleName),
                PersonnelNumber = Normalize(employee.PersonnelNumber),
                Department = Normalize(employee.Department),
                ProfessionId = employee.ProfessionId,
                ProfessionName = Normalize(employee.ProfessionName)
            };
        }

        public bool Equals(EmployeeIdentitySnapshot? other)
        {
            if (other is null)
                return false;

            return LastName == other.LastName
                && FirstName == other.FirstName
                && MiddleName == other.MiddleName
                && PersonnelNumber == other.PersonnelNumber
                && Department == other.Department
                && ProfessionId == other.ProfessionId
                && ProfessionName == other.ProfessionName;
        }

        public override bool Equals(object? obj)
        {
            return obj is EmployeeIdentitySnapshot other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                LastName,
                FirstName,
                MiddleName,
                PersonnelNumber,
                Department,
                ProfessionId,
                ProfessionName);
        }

        private static string Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}

/// <summary>
/// Row model for the SIZ DataGrid (editable).
/// </summary>
public partial class EmployeeSizRow : ObservableObject
{
    [ObservableProperty] private string _type = string.Empty;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _norm = string.Empty;
}
