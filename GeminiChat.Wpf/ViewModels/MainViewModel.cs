using GeminiChat.Core;
using GeminiChat.Wpf.Commands;
using GeminiChat.Wpf.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GeminiChat.Wpf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly IChatService _chatService;
        private readonly ChatHistoryManager _chatHistoryManager;
        private readonly IServiceProvider _serviceProvider;
        private string _userInput = string.Empty;
        private bool _isThinking;

        public ObservableCollection<ChatMessage> ChatHistory { get; set; }
        public SettingsManager Settings { get; }

        public string UserInput
        {
            get => _userInput;
            set
            {
                _userInput = value;
                OnPropertyChanged();
            }
        }

        public bool IsThinking
        {
            get => _isThinking;
            private set
            {
                _isThinking = value;
                OnPropertyChanged();
            }
        }

        public ICommand SendCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand NewChatCommand { get; }

        public MainViewModel(ILogger logger, IChatService chatService, SettingsManager settings, ChatHistoryManager chatHistoryManager, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _chatService = chatService;
            Settings = settings;
            _chatHistoryManager = chatHistoryManager;
            _serviceProvider = serviceProvider;

            ChatHistory = _chatHistoryManager.LoadHistory();
            _logger.LogInfo($"{ChatHistory.Count} messages loaded from local file.");

            ChatHistory.CollectionChanged += (s, e) => _chatHistoryManager.SaveHistory(ChatHistory);

            SendCommand = new RelayCommand(
                execute: async _ => await OnSend(),
                canExecute: _ => !string.IsNullOrWhiteSpace(UserInput) && !IsThinking
            );

            OpenSettingsCommand = new RelayCommand(_ => OnOpenSettings());

            NewChatCommand = new RelayCommand(_ => OnNewChat());
        }

        private void OnNewChat()
        {
            _logger.LogInfo("User requested a new chat.");

            // 1. Очищаем коллекцию сообщений в интерфейсе.
            // Это также приведет к сохранению пустой истории в файл.
            ChatHistory.Clear();

            // 2. Говорим сервису сбросить свою сессию.
            _chatService.StartNewChat();

            _logger.LogInfo("Chat history and session have been cleared.");
        }

        public async Task InitializeAsync()
        {
            _logger.LogInfo("ViewModel asynchronous initialization started...");
            await _chatService.PrimeContextAsync(this.ChatHistory);
            _logger.LogInfo("ViewModel asynchronous initialization finished.");
        }

        private void OnOpenSettings()
        {
            _logger.LogInfo("Opening settings window...");
            var settingsWindow = new SettingsWindow
            {
                DataContext = _serviceProvider.GetRequiredService<SettingsViewModel>(),
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
