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
            if (item is ChatMessage message && message.Content.Trim().StartsWith("```"))
            {
                return CodeTemplate;
            }

            return PlainTextTemplate;
        }
    }
}
