// Services/DialogService.cs
using GeminiChat.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace GeminiChat.Wpf.Services
{
    public class DialogService : IDialogService
    {
        private readonly IServiceProvider _serviceProvider;

        public DialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowSettingsDialog()
        {
            // Мы получаем новый экземпляр ViewModel и Window каждый раз, когда открываем окно
            var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
            var settingsWindow = new SettingsWindow(settingsViewModel)
            {
                // Устанавливаем владельца, чтобы окно открылось по центру главного
                Owner = Application.Current.MainWindow
            };

            settingsWindow.ShowDialog();

            // После закрытия окна, можно обновить настройки в главном ViewModel
            // Это более сложная тема (мессенджер или события), пока оставим так.
            // Пользователь должен будет перезапустить приложение, чтобы увидеть новые шрифты.
        }
    }
}
