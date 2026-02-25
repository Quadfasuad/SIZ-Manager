using System.Windows;
using Microsoft.Win32;

namespace SizManager.Helpers;

public class DialogService
{
    public string? OpenFileDialog(string filter, string title = "Открыть файл")
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            Title = title
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? SaveFileDialog(string filter, string defaultFileName = "", string title = "Сохранить файл")
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            Title = title,
            FileName = defaultFileName
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public void ShowMessage(string message, string title = "Информация")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message, string title = "Ошибка")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowWarning(string message, string title = "Предупреждение")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    public bool ShowConfirmation(string message, string title = "Подтверждение")
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
               == MessageBoxResult.Yes;
    }
}
