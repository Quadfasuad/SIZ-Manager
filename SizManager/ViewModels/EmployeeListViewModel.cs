using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using SizManager.Helpers;
using SizManager.Services.Database;

namespace SizManager.ViewModels;

public partial class EmployeeListViewModel : ObservableObject
{
    private readonly DialogService _dialogService;

    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ObservableCollection<EmployeeListItem> _employees = new();
    [ObservableProperty] private EmployeeListItem? _selectedEmployee;
    [ObservableProperty] private int _totalCount;

    public int SelectedEmployeeId { get; private set; }

    public EmployeeListViewModel(DialogService dialogService)
    {
        _dialogService = dialogService;
        LoadEmployees();
    }

    partial void OnSearchTextChanged(string value)
    {
        LoadEmployees();
    }

    private void LoadEmployees()
    {
        try
        {
            using var context = new SizDbContext();
            var query = context.Employees.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.Trim();
                query = query.Where(e =>
                    e.LastName.Contains(search) ||
                    e.FirstName.Contains(search) ||
                    (e.MiddleName != null && e.MiddleName.Contains(search)) ||
                    (e.PersonnelNumber != null && e.PersonnelNumber.Contains(search)) ||
                    e.ProfessionName.Contains(search));
            }

            var list = query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .Select(e => new EmployeeListItem
                {
                    Id = e.Id,
                    FullName = e.LastName + " " + e.FirstName + (e.MiddleName != null ? " " + e.MiddleName : ""),
                    ProfessionName = e.ProfessionName,
                    PersonnelNumber = e.PersonnelNumber ?? "",
                    Department = e.Department ?? ""
                })
                .ToList();

            Employees = new ObservableCollection<EmployeeListItem>(list);
            TotalCount = list.Count;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "LoadEmployees");
        }
    }

    [RelayCommand]
    private void OpenSelected()
    {
        if (SelectedEmployee == null) return;
        SelectedEmployeeId = SelectedEmployee.Id;
    }

    [RelayCommand]
    private void DeleteSelected()
    {
        if (SelectedEmployee == null) return;

        if (!_dialogService.ShowConfirmation(
            $"Удалить карточку \"{SelectedEmployee.FullName}\"?"))
            return;

        try
        {
            using var context = new SizDbContext();
            var employee = context.Employees.Find(SelectedEmployee.Id);
            if (employee != null)
            {
                context.Employees.Remove(employee);
                context.SaveChanges();
            }

            LoadEmployees();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "DeleteEmployee");
            _dialogService.ShowError($"Ошибка удаления: {ex.Message}");
        }
    }
}

public class EmployeeListItem
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string ProfessionName { get; set; } = string.Empty;
    public string PersonnelNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}
