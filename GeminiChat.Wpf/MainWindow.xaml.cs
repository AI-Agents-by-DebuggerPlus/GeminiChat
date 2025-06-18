// MainWindow.xaml.cs
using GeminiChat.Wpf.ViewModels;
using System;
using System.Windows;

namespace GeminiChat.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Подписываемся на событие, когда окно полностью загружено
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Убеждаемся, что DataContext - это наша MainViewModel
            if (DataContext is MainViewModel viewModel)
            {
                // Подписываемся на событие из ViewModel
                //viewModel.MessageAdded += OnMessageAdded;
            }
        }

        /// <summary>
        /// Этот метод будет вызываться каждый раз, когда ViewModel добавляет сообщение.
        /// </summary>
        //private void OnMessageAdded()
        //{
        //    // Прокручиваем ScrollViewer в самый низ
        //    MessagesScrollViewer.ScrollToEnd();
        //}

        // Отписываемся от события при закрытии окна, чтобы избежать утечек памяти
        //protected override void OnClosed(EventArgs e)
        //{
        //    if (DataContext is MainViewModel viewModel)
        //    {
        //        viewModel.MessageAdded -= OnMessageAdded;
        //    }
        //    base.OnClosed(e);
        //}
    }
}
