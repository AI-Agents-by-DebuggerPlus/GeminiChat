// SettingsWindow.xaml.cs
using GeminiChat.Wpf.ViewModels;
using System.Windows;

namespace GeminiChat.Wpf
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(SettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            // Эта магия позволяет ViewModel'у закрыть окно
            viewModel.CloseAction = new System.Action(this.Close);
        }
    }
}
