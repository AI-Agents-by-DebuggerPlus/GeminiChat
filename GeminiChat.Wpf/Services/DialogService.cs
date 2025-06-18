// Services/DialogService.cs
using GeminiChat.Core; // <-- Важный using для ILogger
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
            var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
            var settingsWindow = new SettingsWindow(settingsViewModel)
            {
                Owner = Application.Current.MainWindow
            };
            settingsWindow.ShowDialog();
        }

        public bool EnsureApiKeyIsSet()
        {
            var settingsManager = _serviceProvider.GetRequiredService<SettingsManager>();
            var logger = _serviceProvider.GetRequiredService<ILogger>();
            var settings = settingsManager.LoadSettings();

            while (string.IsNullOrEmpty(settings.ApiKey))
            {
                // Получаем новый экземпляр ViewModel из контейнера.
                // Контейнер сам внедрит в него SettingsManager и ILogger.
                var settingsViewModel = new SettingsViewModel(settingsManager, logger);
                var settingsWindow = new SettingsWindow(settingsViewModel);

                var dialogResult = settingsWindow.ShowDialog();

                if (dialogResult != true)
                {
                    // Пользователь нажал "Отмена" или закрыл окно
                    return false;
                }

                settings = settingsManager.LoadSettings();
            }

            // Если мы вышли из цикла, значит ключ есть
            return true;
        }
    }
}
