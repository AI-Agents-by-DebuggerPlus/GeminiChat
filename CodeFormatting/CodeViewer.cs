using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows;
using System.Windows.Controls;

// Убедитесь, что пространство имен совпадает с названием проекта
namespace CodeFormatting
{
    /// <summary>
    /// A custom control for displaying code with syntax highlighting using AvalonEdit.
    /// </summary>
    public class CodeViewer : Control
    {
        private TextEditor _textEditor;

        // Определяем стиль по умолчанию
        static CodeViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CodeViewer), new FrameworkPropertyMetadata(typeof(CodeViewer)));
        }

        // --- DependencyProperty для привязки текста кода ---
        public static readonly DependencyProperty TextContentProperty =
            DependencyProperty.Register("TextContent", typeof(string), typeof(CodeViewer),
                new PropertyMetadata(string.Empty, OnTextContentChanged));

        public string TextContent
        {
            get { return (string)GetValue(TextContentProperty); }
            set { SetValue(TextContentProperty, value); }
        }

        // --- Переопределение метода для получения доступа к элементам из шаблона ---
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _textEditor = GetTemplateChild("PART_TextEditor") as TextEditor;
            UpdateTextEditor();
        }

        private static void OnTextContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CodeViewer codeViewer)
            {
                codeViewer.UpdateTextEditor();
            }
        }

        private void UpdateTextEditor()
        {
            if (_textEditor == null) return;

            _textEditor.Text = TextContent ?? string.Empty;
            // По умолчанию ставим подсветку C#. Позже можно будет сделать это свойством.
            _textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
        }
    }
}
