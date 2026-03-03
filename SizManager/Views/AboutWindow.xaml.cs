using System.Reflection;
using System.Windows;

namespace SizManager.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        var v = Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = $"Версия {v?.Major}.{v?.Minor}.{v?.Build}";
    }
}
