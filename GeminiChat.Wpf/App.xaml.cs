using GeminiChat.Core;
using GeminiChat.Gemini;
using GeminiChat.Logging;
using GeminiChat.Wpf.ViewModels;
using Microsoft.Extensions.Configuration; // <-- Добавить
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.IO; // <-- Добавить
using System.Windows;

namespace GeminiChat.Wpf
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration; // <-- Добавить

        public App()
        {
            // Настраиваем чтение конфигурации из файла
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _configuration = builder.Build();


            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILogger, DebugLogger>();

            // Изменяем регистрацию сервиса, чтобы передать ему ключ из конфигурации
            services.AddSingleton<IChatService>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger>();
                var apiKey = _configuration.GetValue<string>("Gemini:ApiKey");
                return new GeminiChatService(logger, apiKey);
            });

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.DataContext = _serviceProvider.GetService<MainViewModel>();
            mainWindow.Show();
        }
    }
}