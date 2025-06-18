// App.xaml.cs
using GeminiChat.Core;
using GeminiChat.Gemini;
using GeminiChat.Logging;
using GeminiChat.Wpf.Services;
using GeminiChat.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics; // <-- Важный using для работы с процессами
using System.Windows;
using System.Windows.Threading;

namespace GeminiChat.Wpf
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var logDirectory = "d:\\Programming\\Debug\\Logs\\GeminiChat\\";
            var systemLogger = new FileLogger(System.IO.Path.Combine(logDirectory, "system.log"), clearOnStartup: true);

            // Настраиваем глобальные обработчики ошибок
            SetupExceptionHandlers(systemLogger);

            var settingsManager = new SettingsManager();
            var settings = settingsManager.LoadSettings();

            // Проверяем, есть ли валидный ключ
            if (string.IsNullOrEmpty(settings.ApiKey))
            {
                systemLogger.LogInfo("API Key is missing. Showing settings window.");
                var settingsViewModel = new SettingsViewModel(settingsManager, systemLogger);
                var settingsWindow = new SettingsWindow(settingsViewModel);

                var dialogResult = settingsWindow.ShowDialog();

                if (dialogResult == true)
                {
                    // --- ГЛАВНОЕ ИЗМЕНЕНИЕ: ЛОГИКА ПЕРЕЗАПУСКА ---
                    systemLogger.LogInfo("API Key saved. Restarting the application...");

                    // Получаем путь к текущему запущенному .exe файлу
                    var processPath = Process.GetCurrentProcess().MainModule?.FileName;
                    if (processPath != null)
                    {
                        // Запускаем новую копию приложения
                        Process.Start(processPath);
                    }

                    // Немедленно закрываем текущий "сломанный" процесс
                    Shutdown();
                    return; // Выходим, чтобы не продолжать выполнение
                }
                else
                {
                    // Пользователь нажал "Отмена" или закрыл окно
                    systemLogger.LogInfo("User cancelled API key entry. Shutting down.");
                    Shutdown();
                    return;
                }
            }

            // Этот код выполнится, только если ключ уже был (т.е. при втором, "чистом" запуске)
            systemLogger.LogInfo("API Key found. Starting application normally.");
            var services = new ServiceCollection();
            ConfigureServices(services, settings, logDirectory);
            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }

        private void SetupExceptionHandlers(ILogger logger)
        {
            // Обработчик для ошибок в UI-потоке
            this.Dispatcher.UnhandledException += (s, e) =>
            {
                logger.LogError("A UI Dispatcher unhandled exception occurred", e.Exception);
                e.Handled = true;
            };

            // Обработчик для всех остальных ошибок
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    logger.LogError("An AppDomain unhandled exception occurred", ex);
                }
            };
        }

        private void ConfigureServices(IServiceCollection services, AppSettings settings, string logDirectory)
        {
            services.AddSingleton<ILogger>(new FileLogger(System.IO.Path.Combine(logDirectory, "system.log")));
            services.AddSingleton(settings);
            services.AddSingleton<SettingsManager>();

            services.AddSingleton<IChatService>(provider =>
            {
                var chatLogger = new FileLogger(System.IO.Path.Combine(logDirectory, "chat.log"));
                return new GeminiChatService(settings.ApiKey!, chatLogger);
            });

            services.AddSingleton<ChatHistoryManager>();
            services.AddSingleton<IDialogService, DialogService>();

            services.AddTransient<MainViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<MainWindow>();
        }
    }
}
