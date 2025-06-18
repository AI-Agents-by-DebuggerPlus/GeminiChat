// SettingsWindow.xaml.cs
using GeminiChat.Wpf.ViewModels;
using System;
using System.Windows;

namespace GeminiChat.Wpf
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(SettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Этот механизм позволяет ViewModel'у закрыть окно
            // и сообщить родительскому коду, что все прошло успешно.
            if (viewModel.CloseAction == null)
            {
                // Для команды "Сохранить"
                viewModel.CloseAction = () =>
                {
                    // Устанавливаем результат "Успех"
                    this.DialogResult = true;
                    this.Close();
                };
            }
        }
    }
}
