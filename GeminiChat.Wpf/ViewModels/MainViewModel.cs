using GeminiChat.Core;
using GeminiChat.Wpf.Commands;
using GeminiChat.Wpf.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows; // <-- ВОТ ИСПРАВЛЕНИЕ
using System.Windows.Input;

namespace GeminiChat.Wpf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly IChatService _chatService;
        private readonly IServiceProvider _serviceProvider;
        private string _userInput = string.Empty;
        private bool _isThinking;

        /// <summary>
        /// Коллекция для хранения истории чата.
        /// </summary>
        public ObservableCollection<ChatMessage> ChatHistory { get; } = new();

        /// <summary>
        /// Сервис для доступа к глобальным настройкам приложения.
        /// </summary>
        public SettingsManager Settings { get; }

        /// <summary>
        /// Текст, который пользователь вводит в данный момент.
        /// </summary>
        public string UserInput
        {
            get => _userInput;
            set
            {
                _userInput = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Флаг, показывающий, что приложение ожидает ответа от API.
        /// </summary>
        public bool IsThinking
        {
            get => _isThinking;
            private set
            {
                _isThinking = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Команда для отправки сообщения.
        /// </summary>
        public ICommand SendCommand { get; }

        /// <summary>
        /// Команда для открытия окна настроек.
        /// </summary>
        public ICommand OpenSettingsCommand { get; }

        public MainViewModel(ILogger logger, IChatService chatService, SettingsManager settings, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _chatService = chatService;
            Settings = settings;
            _serviceProvider = serviceProvider;

            _logger.LogInfo("MainViewModel created and services injected.");

            SendCommand = new RelayCommand(
                execute: async _ => await OnSend(),
                canExecute: _ => !string.IsNullOrWhiteSpace(UserInput) && !IsThinking
            );

            OpenSettingsCommand = new RelayCommand(_ => OnOpenSettings());
        }

        private void OnOpenSettings()
        {
            _logger.LogInfo("Opening settings window...");
            // Создаем новое окно настроек
            var settingsWindow = new SettingsWindow
            {
                // Задаем ему в качестве DataContext новую ViewModel, полученную из DI-контейнера
                DataContext = _serviceProvider.GetRequiredService<SettingsViewModel>(),
                // Устанавливаем владельца, чтобы окно открылось по центру главного
                Owner = Application.Current.MainWindow
            };
            settingsWindow.ShowDialog();
        }

        private async Task OnSend()
        {
            string messageToSend = UserInput;

            ChatHistory.Add(new ChatMessage(Author.User, messageToSend));
            UserInput = string.Empty;

            IsThinking = true;
            _logger.LogInfo($"User message to be sent: {messageToSend}");

            try
            {
                var responseText = await _chatService.SendMessageAsync(messageToSend);
                ChatHistory.Add(new ChatMessage(Author.Model, responseText));
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get response from chat service.", ex);
                ChatHistory.Add(new ChatMessage(Author.Model, "Извините, произошла ошибка."));
            }
            finally
            {
                IsThinking = false;
            }
        }
    }
}
