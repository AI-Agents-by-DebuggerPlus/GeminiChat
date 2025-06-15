using GeminiChat.Wpf.ViewModels;
using System.Windows;

namespace GeminiChat.Wpf
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            // Подписываемся на событие Loaded, которое сработает, когда окно будет полностью готово
            this.Loaded += SettingsWindow_Loaded;
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Когда окно загрузилось, мы можем безопасно работать с его DataContext (ViewModel)
            if (DataContext is SettingsViewModel viewModel)
            {
                // Мы говорим ViewModel: "Когда тебе нужно будет закрыться, просто вызови метод this.Close()"
                viewModel.CloseAction = () => this.Close();
            }
        }
    }
}
