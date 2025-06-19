// Converters/NullToVisibilityConverter.cs
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GeminiChat.Wpf.Converters
{
    /// <summary>
    /// Преобразует null в Visibility.Collapsed, а любой другой объект в Visibility.Visible.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
