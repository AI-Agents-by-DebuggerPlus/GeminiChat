// Selectors/MessageTemplateSelector.cs
using GeminiChat.Core;
using System.Windows;
using System.Windows.Controls;

namespace GeminiChat.Wpf.Selectors
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        // Добавляем свойства для всех наших шаблонов
        public DataTemplate? PlainTextTemplate { get; set; }
        public DataTemplate? CodeTemplate { get; set; }
        public DataTemplate? InstructionTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is ChatMessage message && !string.IsNullOrEmpty(message.Content))
            {
                var trimmedContent = message.Content.Trim();

                // 1. Сначала проверяем на код
                if (trimmedContent.StartsWith("```"))
                {
                    return CodeTemplate;
                }

                // 2. Затем проверяем на инструкцию/список
                // Мы используем PlainTextTemplate и для инструкций, и для списков,
                // так как форматирование `inline-кода` происходит внутри него.
                // Если бы у нас был совершенно другой вид для инструкций, мы бы вернули InstructionTemplate.
                // В вашем случае, этот блок можно даже упростить, но оставим для гибкости.
                if (trimmedContent.StartsWith("*") || trimmedContent.StartsWith("-"))
                {
                    return PlainTextTemplate; // Используем тот же шаблон, что и для обычного текста
                }
            }

            // 3. Во всех остальных случаях - обычный текст
            return PlainTextTemplate;
        }
    }
}
