// Converters/AuthorToAlignmentConverter.cs
using GeminiChat.Core;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GeminiChat.Wpf.Converters
{
    public class AuthorToAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Author author)
            {
                return author == Author.User ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            }
            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
