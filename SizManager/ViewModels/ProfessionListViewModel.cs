using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using SizManager.Helpers;
using SizManager.Models;
using SizManager.Services.Database;

namespace SizManager.ViewModels;

public partial class ProfessionListViewModel : ObservableObject
{
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private ObservableCollection<Profession> _professions = new();
    [ObservableProperty] private Profession? _selectedProfession;
    [ObservableProperty] private ObservableCollection<ProfessionSIZ> _sizItems = new();
    [ObservableProperty] private int _totalCount;

    public ProfessionListViewModel()
    {
        LoadProfessions();
    }

    partial void OnSearchTextChanged(string value)
    {
        LoadProfessions();
    }

    partial void OnSelectedProfessionChanged(Profession? value)
    {
        LoadSizItems();
    }

    private void LoadProfessions()
    {
        try
        {
            using var context = new SizDbContext();
            var query = context.Professions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.Trim();
                query = query.Where(p => p.Name.Contains(search) || p.Number.Contains(search));
            }

            var list = query.OrderBy(p => p.Name).ToList();
            Professions = new ObservableCollection<Profession>(list);
            TotalCount = list.Count;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "LoadProfessions");
        }
    }

    private void LoadSizItems()
    {
        SizItems.Clear();
        if (SelectedProfession == null) return;

        try
        {
            using var context = new SizDbContext();
            var items = context.ProfessionSIZ
                .Where(s => s.ProfessionId == SelectedProfession.Id)
                .ToList();

            SizItems = new ObservableCollection<ProfessionSIZ>(items);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "LoadSizItems");
        }
    }
}
