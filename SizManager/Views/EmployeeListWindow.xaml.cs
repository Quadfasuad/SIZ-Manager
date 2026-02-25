using System.Windows;
using System.Windows.Input;
using SizManager.ViewModels;

namespace SizManager.Views;

public partial class EmployeeListWindow : Window
{
    public EmployeeListWindow()
    {
        InitializeComponent();
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        SelectAndClose();
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        SelectAndClose();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void SelectAndClose()
    {
        if (DataContext is EmployeeListViewModel vm && vm.SelectedEmployee != null)
        {
            vm.OpenSelectedCommand.Execute(null);
            DialogResult = true;
            Close();
        }
    }
}
