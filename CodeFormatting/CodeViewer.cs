// CodeFormatting/CodeViewer.cs
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows;
using System.Windows.Controls;

namespace CodeFormatting
{
    public class CodeViewer : Control
    {
        private TextEditor? _textEditor;

        static CodeViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CodeViewer), new FrameworkPropertyMetadata(typeof(CodeViewer)));
        }

        public static readonly DependencyProperty TextContentProperty =
            DependencyProperty.Register("TextContent", typeof(string), typeof(CodeViewer),
                new PropertyMetadata(string.Empty, OnTextContentChanged));

        public string TextContent
        {
            get { return (string)GetValue(TextContentProperty); }
            set { SetValue(TextContentProperty, value); }
        }

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
            _textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
        }
    }
}
