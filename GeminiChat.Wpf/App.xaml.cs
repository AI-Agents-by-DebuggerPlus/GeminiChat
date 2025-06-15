using GeminiChat.Core;
using GeminiChat.Gemini;
using GeminiChat.Logging;
using GeminiChat.Wpf.Services;
using GeminiChat.Wpf.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace GeminiChat.Wpf
{
    /// <summary>
    /// Главный класс приложения, отвечающий за запуск и конфигурацию.
    /// </summary>
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

            // Регистрируем ChatHistoryManager через фабрику, чтобы передать ему логгер
            services.AddSingleton(provider => new ChatHistoryManager(
                provider.GetRequiredService<ILogger>()
            ));

            // Регистрируем IChatService с использованием фабрики, чтобы передать ему ключ API из конфигурации
            services.AddSingleton<IChatService>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger>();
                var apiKey = _configuration.GetValue<string>("Gemini:ApiKey");
                return new GeminiChatService(logger, apiKey);
            });

            // Регистрируем MainViewModel и передаем ему все необходимые зависимости
            services.AddSingleton(provider => new MainViewModel(
                provider.GetRequiredService<ILogger>(),
                provider.GetRequiredService<IChatService>(),
                provider.GetRequiredService<SettingsManager>(),
                provider.GetRequiredService<ChatHistoryManager>(),
                provider // Передаем сам IServiceProvider для создания дочерних окон
            ));

            // Регистрируем главное окно
            services.AddSingleton<MainWindow>();

            // Регистрируем ViewModel для окна настроек как Transient (новый экземпляр каждый раз)
            services.AddTransient<SettingsViewModel>();
        }

        /// <summary>
        /// Этот метод вызывается при запуске приложения.
        /// Он стал асинхронным, чтобы дождаться "заправки" контекста модели.
        /// </summary>
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Получаем ViewModel из DI-контейнера
            var viewModel = _serviceProvider.GetService<MainViewModel>();

            // 2. Выполняем асинхронную инициализацию ViewModel
            // Приложение "подождет" здесь, пока не выполнится PrimeContextAsync
            await viewModel.InitializeAsync();

            // 3. Получаем главное окно
            var mainWindow = _serviceProvider.GetService<MainWindow>();

            // 4. Устанавливаем DataContext, который теперь полностью готов
            mainWindow.DataContext = viewModel;

            // 5. Показываем окно
            mainWindow.Show();
        }
    }
}
