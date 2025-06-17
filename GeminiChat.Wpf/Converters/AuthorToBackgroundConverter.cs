// Converters/AuthorToBackgroundConverter.cs
using GeminiChat.Core;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GeminiChat.Wpf.Converters
{
    public class AuthorToBackgroundConverter : IValueConverter
    {
        // Цвета можно вынести в ресурсы для удобства
        private readonly SolidColorBrush _userBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#004D40")); // Темно-бирюзовый
        private readonly SolidColorBrush _modelBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#37474F")); // Темно-серый

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Author author)
            {
                return author == Author.User ? _userBrush : _modelBrush;
            }
            return _modelBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
