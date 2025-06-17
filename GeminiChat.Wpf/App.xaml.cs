// App.xaml.cs
using GeminiChat.Core;
using GeminiChat.Gemini;
using GeminiChat.Logging;
using GeminiChat.Wpf.Services;
using GeminiChat.Wpf.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
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
            var logger = _serviceProvider.GetRequiredService<ILogger>();
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    logger.LogError("An unhandled exception occurred", ex);
                }
            };

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            // ViewModel создается и присваивается здесь, со всеми зависимостями.
            mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<ILogger>(new FileLogger("d:\\Programming\\Debug\\Logs\\GeminiChat\\"));

            services.AddSingleton<IChatService>(provider => {
                var apiKey = provider.GetRequiredService<IConfiguration>()["Gemini:ApiKey"];
                if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
                {
                    MessageBox.Show("Gemini API Key is not configured in appsettings.json.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw new InvalidOperationException("Gemini API Key is missing or not set.");
                }
                return new GeminiChatService(apiKey, provider.GetRequiredService<ILogger>());
            });

            // Регистрация сервисов
            services.AddSingleton<ChatHistoryManager>();
            services.AddSingleton<SettingsManager>();
            services.AddSingleton<IDialogService, DialogService>();

            // Регистрация ViewModels и Views
            services.AddTransient<MainViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<MainWindow>();
            // SettingsWindow создается вручную в DialogService, регистрировать его как отдельный сервис не нужно.
        }
    }
}
