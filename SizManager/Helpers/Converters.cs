using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SizManager.Helpers;

/// <summary>
/// Converts a gender string to a boolean for RadioButton binding.
/// ConverterParameter is the expected gender value (e.g., "М" or "Ж").
/// </summary>
public class GenderConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string gender && parameter is string expected && gender == expected;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is true && parameter is string gender)
            return gender;
        return Binding.DoNothing;
    }
}

/// <summary>
/// Converts boolean to Visibility.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility.Visible;
    }
}
