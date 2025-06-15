using GeminiChat.Wpf.ViewModels;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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
            // Подписываемся на событие Loaded, чтобы выполнить действия после полной загрузки окна.
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                // Подписываемся на событие изменения коллекции для автопрокрутки при новых сообщениях.
                viewModel.ChatHistory.CollectionChanged += ChatHistory_CollectionChanged;

                // Если история чата не пуста после загрузки, прокручиваем вниз.
                if (viewModel.ChatHistory.Any())
                {
                    // Используем Dispatcher, чтобы прокрутка сработала после отрисовки элементов.
                    Dispatcher.BeginInvoke(() =>
                    {
                        ChatScrollViewer.ScrollToBottom();
                    });
                }
            }

            // Устанавливаем фокус на поле ввода, когда окно загружено.
            UserInputTextBox.Focus();
        }

        private void ChatHistory_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Если в коллекцию был добавлен новый элемент, прокручиваем вниз.
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ChatScrollViewer.ScrollToBottom();
                });
            }
        }

        private void UserInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Shift + Enter = новая строка (стандартное поведение).
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    return;
                }

                // Только Enter = отправить сообщение.
                if (DataContext is MainViewModel viewModel)
                {
                    if (viewModel.SendCommand.CanExecute(null))
                    {
                        viewModel.SendCommand.Execute(null);
                    }
                }
                e.Handled = true; // Помечаем, чтобы Enter не добавил пустую строку.
            }
        }
    }
}
