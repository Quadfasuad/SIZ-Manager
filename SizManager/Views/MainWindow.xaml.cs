using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SizManager.ViewModels;

namespace SizManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void ProfessionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem != null)
        {
            // Focus back on the text box after selection
            ProfessionTextBox.Focus();
            ProfessionTextBox.CaretIndex = ProfessionTextBox.Text.Length;
        }
    }

    private void ProfessionListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainViewModel vm && vm.Card.SelectedProfession != null)
        {
            vm.Card.IsDropDownOpen = false;
            ProfessionTextBox.Focus();
            ProfessionTextBox.CaretIndex = ProfessionTextBox.Text.Length;
        }
    }
}
