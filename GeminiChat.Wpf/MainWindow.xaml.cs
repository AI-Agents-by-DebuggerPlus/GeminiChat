using System.Windows;
using System.Windows.Input; // Required for key events

namespace GeminiChat.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UserInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Проверяем, нажат ли Enter
            if (e.Key == Key.Enter)
            {
                // Если одновременно с Enter нажат Shift, ничего не делаем.
                // TextBox сам добавит новую строку.
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    return;
                }

                // Если нажат только Enter, мы отправляем сообщение.
                // Получаем доступ к нашей ViewModel из DataContext окна.
                if (DataContext is ViewModels.MainViewModel viewModel)
                {
                    // Проверяем, может ли команда выполниться (например, поле ввода не пустое).
                    if (viewModel.SendCommand.CanExecute(null))
                    {
                        // Выполняем команду, которая находится в ViewModel.
                        viewModel.SendCommand.Execute(null);
                    }
                }

                // Помечаем событие как обработанное.
                // Это не дает TextBox'у обработать Enter и добавить лишний перенос строки.
                e.Handled = true;
            }
        }
    }
}