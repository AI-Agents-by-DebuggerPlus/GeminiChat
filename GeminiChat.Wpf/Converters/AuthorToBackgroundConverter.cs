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
        // Определяем цвета для каждого типа автора
        private readonly SolidColorBrush _userBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#004D40")); // Темно-бирюзовый
        private readonly SolidColorBrush _modelBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#37474F")); // Темно-серый
        private readonly SolidColorBrush _systemBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A4A4A")); // Нейтральный серый

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Author author)
            {
                switch (author)
                {
                    case Author.User:
                        return _userBrush;
                    case Author.Model:
                        return _modelBrush;
                    case Author.System:
                        return _systemBrush; // Возвращаем новый цвет для системных сообщений
                    default:
                        return _modelBrush;
                }
            }
            return _modelBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
