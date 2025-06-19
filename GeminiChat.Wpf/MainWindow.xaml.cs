// MainWindow.xaml.cs
using GeminiChat.Wpf.ViewModels;
using System;
using System.Windows;
using System.Windows.Threading; // <-- Важный using для DispatcherPriority

namespace GeminiChat.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Мы больше не используем событие Loaded, чтобы избежать гонки состояний.
            // Вместо этого мы подписываемся на изменение DataContext.
            this.DataContextChanged += MainWindow_DataContextChanged;
        }

        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Отписываемся от старого ViewModel, если он был
            if (e.OldValue is MainViewModel oldViewModel)
            {
                oldViewModel.MessageAdded -= OnMessageAdded;
            }

            // Подписываемся на новый ViewModel
            if (e.NewValue is MainViewModel newViewModel)
            {
                newViewModel.MessageAdded += OnMessageAdded;
            }
        }

        /// <summary>
        /// Этот метод будет вызываться каждый раз, когда ViewModel добавляет сообщение.
        /// </summary>
        private void OnMessageAdded()
        {
            // ИСПРАВЛЕНИЕ: Мы просим диспетчер выполнить прокрутку с низким приоритетом.
            // Это гарантирует, что сначала завершится вся работа по отрисовке и компоновке,
            // и только потом будет выполнена прокрутка.
            MessagesScrollViewer.Dispatcher.BeginInvoke(
                new Action(() => MessagesScrollViewer.ScrollToEnd()),
                DispatcherPriority.ContextIdle);
        }

        // Отписываемся от события при закрытии окна, чтобы избежать утечек памяти
        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.MessageAdded -= OnMessageAdded;
            }
            base.OnClosed(e);
        }
    }
}
