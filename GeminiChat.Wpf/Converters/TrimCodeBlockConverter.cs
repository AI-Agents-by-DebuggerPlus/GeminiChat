// Converters/TrimCodeBlockConverter.cs
using System;
using System.Globalization;
using System.Windows.Data;

namespace GeminiChat.Wpf.Converters
{
    public class TrimCodeBlockConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string content)
            {
                string trimmedContent = content.Trim('`', '\n', '\r', ' ');
                int firstNewLine = trimmedContent.IndexOf('\n');
                // Пытаемся удалить название языка (например "csharp") после ```
                if (firstNewLine > -1 && firstNewLine < 20)
                {
                    string firstLine = trimmedContent.Substring(0, firstNewLine).Trim();
                    if (!firstLine.Contains(" ") && !string.IsNullOrEmpty(firstLine))
                    {
                        return trimmedContent.Substring(firstNewLine + 1);
                    }
                }
                return trimmedContent;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
