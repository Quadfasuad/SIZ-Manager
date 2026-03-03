using System.Diagnostics;
using System.Windows;
using SizManager.Helpers;
using SizManager.Models;
using SizManager.Services;

namespace SizManager.Views;

public partial class UpdateWindow : Window
{
    private readonly UpdateInfo _updateInfo;
    private readonly UpdateService _updateService;

    public UpdateWindow(UpdateInfo updateInfo, UpdateService updateService)
    {
        InitializeComponent();

        _updateInfo = updateInfo;
        _updateService = updateService;

        CurrentVersionText.Text = UpdateService.GetCurrentVersion();
        NewVersionText.Text = updateInfo.Version;
        ChangelogText.Text = string.IsNullOrWhiteSpace(updateInfo.Changelog)
            ? "Нет описания изменений."
            : updateInfo.Changelog;
    }

    private async void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateButton.IsEnabled = false;
        LaterButton.IsEnabled = false;
        ProgressPanel.Visibility = Visibility.Visible;

        try
        {
            var progress = new Progress<int>(percent =>
            {
                ProgressBar.Value = percent;
                ProgressText.Text = $"Скачивание... {percent}%";
            });

            var installerPath = await _updateService.DownloadUpdateAsync(_updateInfo, progress);

            ProgressText.Text = "Запуск установщика...";

            Process.Start(new ProcessStartInfo
            {
                FileName = installerPath,
                UseShellExecute = true
            });

            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Update download");
            ProgressPanel.Visibility = Visibility.Collapsed;
            UpdateButton.IsEnabled = true;
            LaterButton.IsEnabled = true;

            MessageBox.Show(
                $"Не удалось скачать обновление:\n{ex.Message}",
                "Ошибка обновления",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void LaterButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
