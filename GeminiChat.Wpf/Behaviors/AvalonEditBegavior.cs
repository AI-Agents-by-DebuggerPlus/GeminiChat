// Behaviors/AvalonEditBehavior.cs
using ICSharpCode.AvalonEdit;
using System.Windows;

namespace GeminiChat.Wpf.Behaviors
{
    /// <summary>
    /// Простое статическое свойство для надежной привязки текста к AvalonEdit.
    /// </summary>
    public static class AvalonEditBehavior
    {
        public static readonly DependencyProperty BoundTextProperty =
            DependencyProperty.RegisterAttached(
                "BoundText",
                typeof(string),
                typeof(AvalonEditBehavior),
                new PropertyMetadata(null, OnBoundTextChanged));

        public static string GetBoundText(DependencyObject dp)
        {
            return (string)dp.GetValue(BoundTextProperty);
        }

        public static void SetBoundText(DependencyObject dp, string value)
        {
            dp.SetValue(BoundTextProperty, value);
        }

        private static void OnBoundTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Этот метод вызывается, когда свойство BoundText получает новые данные из ViewModel.
            if (d is TextEditor editor)
            {
                // Мы просто напрямую устанавливаем текст в редактор.
                editor.Text = e.NewValue as string ?? string.Empty;
                // И на всякий случай просим UI обновиться.
                editor.UpdateLayout();
            }
        }
    }
}
