// App.xaml.cs
using GeminiChat.Core;
using GeminiChat.Gemini;
using GeminiChat.Logging;
using GeminiChat.Wpf.Services;
using GeminiChat.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

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

            // Логируем необработанные исключения в системный лог
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    systemLogger.LogError("An unhandled exception occurred", ex);
                }
            };

            var settingsManager = new SettingsManager();
            var settings = settingsManager.LoadSettings();

            // Проверяем ключ. Если его нет, показываем окно настроек.
            if (string.IsNullOrEmpty(settings.ApiKey))
            {
                systemLogger.LogInfo("API Key is missing. Showing settings window.");

                // Для окна настроек нам нужен только SettingsManager и системный логгер.
                var settingsViewModel = new SettingsViewModel(settingsManager, systemLogger);
                var settingsWindow = new SettingsWindow(settingsViewModel);

                var dialogResult = settingsWindow.ShowDialog();

                if (dialogResult != true)
                {
                    // Пользователь нажал "Отмена" или закрыл окно.
                    systemLogger.LogInfo("User cancelled API key entry. Shutting down.");
                    Shutdown();
                    return;
                }

                // Если пользователь сохранил ключ, перезагружаем настройки.
                settings = settingsManager.LoadSettings();
                systemLogger.LogInfo("API Key has been set. Proceeding with application startup.");
            }

            // --- Этот код выполнится, только если у нас есть валидный ключ ---
            var services = new ServiceCollection();
            ConfigureServices(services, settings, logDirectory);
            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services, AppSettings settings, string logDirectory)
        {
            // Регистрируем системный логгер как основной ILogger.
            // Он будет использоваться всеми, кто просит ILogger, кроме ChatService.
            services.AddSingleton<ILogger>(new FileLogger(System.IO.Path.Combine(logDirectory, "system.log")));

            // Регистрируем остальные сервисы.
            services.AddSingleton(settings);
            services.AddSingleton<SettingsManager>();

            services.AddSingleton<IChatService>(provider =>
            {
                // Создаем специальный логгер для чата.
                var chatLogger = new FileLogger(System.IO.Path.Combine(logDirectory, "chat.log"));
                // И передаем его в сервис.
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
