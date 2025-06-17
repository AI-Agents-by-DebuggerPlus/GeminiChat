// Selectors/MessageTemplateSelector.cs
using GeminiChat.Core;
using System.Windows;
using System.Windows.Controls;

namespace GeminiChat.Wpf.Selectors
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PlainTextTemplate { get; set; }
        public DataTemplate CodeTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // Проверяем, является ли объект сообщения ChatMessage
            if (item is ChatMessage message)
            {
                // ИСПРАВЛЕНО: Добавляем проверку, что содержимое сообщения не является null
                if (message.Content != null && message.Content.Trim().StartsWith("```"))
                {
                    // Если это блок кода, возвращаем шаблон для кода
                    return CodeTemplate;
                }
            }

            // Во всех остальных случаях (включая сообщения с пустым содержимым)
            // используем шаблон для обычного текста.
            return PlainTextTemplate;
        }
    }
}
