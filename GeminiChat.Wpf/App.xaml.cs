// App.xaml.cs
using Executor; // <-- Важный using для доступа к библиотеке Executor
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
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var dialogService = _serviceProvider.GetRequiredService<IDialogService>();

            if (!dialogService.EnsureApiKeyIsSet())
            {
                Shutdown();
                return;
            }

            var logger = _serviceProvider.GetRequiredService<ILogger>();
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    logger.LogError("An unhandled exception occurred", ex);
                }
            };

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var logDirectory = "d:\\Programming\\Debug\\Logs\\GeminiChat\\";
            services.AddSingleton<ILogger>(new FileLogger(System.IO.Path.Combine(logDirectory, "system.log")));

            var settingsManager = new SettingsManager();
            var settings = settingsManager.LoadSettings();
            services.AddSingleton(settings);
            services.AddSingleton(settingsManager);

            services.AddSingleton<IChatService>(provider =>
            {
                var chatLogger = new FileLogger(System.IO.Path.Combine(logDirectory, "chat.log"));
                return new GeminiChatService(settings.ApiKey!, chatLogger);
            });

            services.AddSingleton<ChatHistoryManager>();
            services.AddSingleton<IDialogService, DialogService>();

            // --- НОВЫЕ СТРОКИ: Регистрация Executor и Interpreter ---
            services.AddSingleton<CommandExecutor>();
            services.AddSingleton<ICommandInterpreter, CommandInterpreter>();

            // Регистрация ViewModels и Views
            services.AddTransient<MainViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<MainWindow>();
        }
    }
}
