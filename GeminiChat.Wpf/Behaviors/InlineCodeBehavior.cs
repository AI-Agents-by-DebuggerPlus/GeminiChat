// Behaviors/InlineCodeBehavior.cs
using GeminiChat.Core;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace GeminiChat.Wpf.Behaviors
{
    public static class InlineCodeBehavior
    {
        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached(
                "FormattedText",
                typeof(string),
                typeof(InlineCodeBehavior),
                new PropertyMetadata(string.Empty, OnFormattedTextChanged));

        public static string GetFormattedText(DependencyObject obj)
        {
            return (string)obj.GetValue(FormattedTextProperty);
        }

        public static void SetFormattedText(DependencyObject obj, string value)
        {
            obj.SetValue(FormattedTextProperty, value);
        }

        private static void OnFormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock textBlock)
            {
                return;
            }

            var formattedText = e.NewValue as string;
            textBlock.Inlines.Clear();

            if (string.IsNullOrEmpty(formattedText))
            {
                return;
            }

            // Регулярное выражение для поиска `кода`
            var regex = new Regex("(`)(.*?)(`)");
            var lastIndex = 0;
            var inlineCodeStyle = textBlock.TryFindResource("InlineCodeStyle") as Style;

            foreach (Match match in regex.Matches(formattedText))
            {
                // Добавляем обычный текст до найденного кода
                if (match.Index > lastIndex)
                {
                    textBlock.Inlines.Add(new Run(formattedText.Substring(lastIndex, match.Index - lastIndex)));
                }

                // Добавляем сам код, применяя к нему стиль
                var inlineRun = new Run(match.Groups[2].Value)
                {
                    Style = inlineCodeStyle
                };
                textBlock.Inlines.Add(inlineRun);

                lastIndex = match.Index + match.Length;
            }

            // Добавляем оставшийся обычный текст после последнего кода
            if (lastIndex < formattedText.Length)
            {
                textBlock.Inlines.Add(new Run(formattedText.Substring(lastIndex)));
            }
        }
    }
}
