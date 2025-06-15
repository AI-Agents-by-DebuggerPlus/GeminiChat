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
        private readonly ServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public App()
        {
            // Настройка чтения конфигурации из файла appsettings.json
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _configuration = builder.Build();

            // Создание коллекции сервисов для Dependency Injection
            var services = new ServiceCollection();
            ConfigureServices(services);

            // Построение провайдера сервисов
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Добавляем саму конфигурацию как сервис, чтобы другие части могли ее получить
            services.AddSingleton<IConfiguration>(_configuration);

            // Регистрируем все наши сервисы как Singleton (один экземпляр на все приложение)
            services.AddSingleton<ILogger, FileLogger>();
            services.AddSingleton<SettingsManager>();
            services.AddSingleton<ChatHistoryManager>(); // <-- Сервис для истории чата

            // Регистрируем IChatService с использованием фабрики, чтобы передать ему ключ API из конфигурации
            services.AddSingleton<IChatService>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger>();
                // Читаем ключ из appsettings.json
                var apiKey = _configuration.GetValue<string>("Gemini:ApiKey");
                return new GeminiChatService(logger, apiKey);
            });

            // Регистрируем MainViewModel и передаем ему все необходимые зависимости
            services.AddSingleton(provider => new MainViewModel(
                provider.GetRequiredService<ILogger>(),
                provider.GetRequiredService<IChatService>(),
                provider.GetRequiredService<SettingsManager>(),
                provider.GetRequiredService<ChatHistoryManager>(), // <-- Передаем сервис истории
                provider // Передаем сам IServiceProvider для создания дочерних окон
            ));

            // Регистрируем главное окно
            services.AddSingleton<MainWindow>();

            // Регистрируем ViewModel для окна настроек как Transient (новый экземпляр каждый раз)
            services.AddTransient<SettingsViewModel>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Получаем MainWindow из нашего DI-контейнера
            var mainWindow = _serviceProvider.GetService<MainWindow>();

            // Устанавливаем DataContext, чтобы окно знало о своей ViewModel
            mainWindow.DataContext = _serviceProvider.GetService<MainViewModel>();

            mainWindow.Show();
        }
    }
}
