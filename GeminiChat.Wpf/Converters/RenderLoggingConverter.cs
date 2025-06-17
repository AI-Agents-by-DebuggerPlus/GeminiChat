// Converters/RenderLoggingConverter.cs
using GeminiChat.Core;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GeminiChat.Wpf.Converters
{
    public class RenderLoggingConverter : IValueConverter
    {
        public static ILogger? Logger { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверяем, что логгер установлен
            if (Logger == null) return value;

            // ИСПРАВЛЕНО: Добавляем проверку, что и сообщение, и его содержимое не равны null
            if (value is ChatMessage message && message.Content != null)
            {
                // Формируем безопасную строку для лога
                string contentPreview = message.Content.Length > 50
                    ? message.Content.Substring(0, 50) + "..."
                    : message.Content;

                Logger.LogInfo($"[RENDER] UI is rendering message from '{message.Author}': \"{contentPreview}\"");
            }
            else if (value is ChatMessage)
            {
                // Логируем случай, когда сообщение есть, а текста в нем нет
                Logger.LogWarning("[RENDER] UI is attempting to render a message with null content.");
            }

            // Мы ничего не меняем, просто возвращаем исходное значение
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
