// ViewModels/MainViewModel.cs
using GeminiChat.Core;
using GeminiChat.Wpf.Commands;
using GeminiChat.Wpf.Messaging; // <-- Важный using
using GeminiChat.Wpf.Services;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace GeminiChat.Wpf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        // --- Сервисы ---
        private readonly IChatService? _chatService;
        private readonly ILogger? _logger;
        private readonly ChatHistoryManager? _chatHistoryManager;
        private readonly IDialogService? _dialogService;
        private readonly SettingsManager? _settingsManager;

        // --- Поля для свойств ---
        private string _currentMessage = "";
        private bool _isSending;
        private string _fontFamily = "Segoe UI";
        private double _fontSize = 15;

        // --- Свойства для привязки к UI ---
        public ObservableCollection<ChatMessage> Messages { get; }
        public string CurrentMessage { get => _currentMessage; set => SetProperty(ref _currentMessage, value); }
        public bool IsSending { get => _isSending; set => SetProperty(ref _isSending, value); }

        // НОВЫЕ СВОЙСТВА ДЛЯ ШРИФТОВ, к которым будет привязано главное окно
        public string FontFamily { get => _fontFamily; set => SetProperty(ref _fontFamily, value); }
        public double FontSize { get => _fontSize; set => SetProperty(ref _fontSize, value); }

        public ICommand SendMessageCommand { get; }
        public ICommand NewChatCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public event Action? MessageAdded;

        // Конструктор для дизайнера
        public MainViewModel()
        {
            Messages = new ObservableCollection<ChatMessage>();
            SendMessageCommand = new RelayCommand(_ => { }, _ => false);
            NewChatCommand = new RelayCommand(_ => { });
            OpenSettingsCommand = new RelayCommand(_ => { });
        }

        // Основной конструктор
        public MainViewModel(IChatService chatService, ILogger logger, ChatHistoryManager chatHistoryManager, IDialogService dialogService, SettingsManager settingsManager)
        {
            _chatService = chatService;
            _logger = logger;
            _chatHistoryManager = chatHistoryManager;
            _dialogService = dialogService;
            _settingsManager = settingsManager;

            Messages = new ObservableCollection<ChatMessage>();
            SendMessageCommand = new RelayCommand(async _ => await SendMessageAsync(), _ => !string.IsNullOrWhiteSpace(CurrentMessage) && !IsSending);
            NewChatCommand = new RelayCommand(_ => NewChat());
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());

            // --- ПОДПИСКА НА СООБЩЕНИЯ ---
            Messenger.Register<SettingsUpdatedMessage>(OnSettingsUpdated);

            LoadFontSettings(); // Загружаем шрифты при старте
            LoadHistory();
        }

        /// <summary>
        /// Метод, который вызывается, когда окно настроек сообщает об изменениях.
        /// </summary>
        private void OnSettingsUpdated(SettingsUpdatedMessage message)
        {
            _logger?.LogInfo("Received settings update message. Applying new fonts.");
            LoadFontSettings();
        }

        /// <summary>
        /// Загружает настройки из файла и применяет их к свойствам ViewModel.
        /// </summary>
        private void LoadFontSettings()
        {
            if (_settingsManager == null) return;
            var settings = _settingsManager.LoadSettings();
            FontFamily = settings.FontFamily;
            FontSize = settings.FontSize;
        }

        // ... (остальные методы, такие как SendMessageAsync, LoadHistory и т.д., остаются без изменений)
        private void AddMessageToCollection(ChatMessage message)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Messages.Add(message);
                MessageAdded?.Invoke();
            }));
        }

        private async Task SendMessageAsync()
        {
            if (_chatService == null || _chatHistoryManager == null) return;
            var userMessageContent = CurrentMessage;
            CurrentMessage = string.Empty;
            var userMessage = new ChatMessage(Author.User, userMessageContent);
            AddMessageToCollection(userMessage);
            _chatHistoryManager.SaveHistory(Messages);
            IsSending = true;
            try
            {
                var responseText = await Task.Run(() => _chatService.SendMessageAsync(userMessageContent));
                ProcessAndDisplayResponse(responseText);
                _chatHistoryManager.SaveHistory(Messages);
            }
            finally { IsSending = false; }
        }

        private void ProcessAndDisplayResponse(string response)
        {
            var codeBlockPattern = @"(```[\s\S]*?```)";
            var parts = Regex.Split(response, codeBlockPattern);
            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part)) continue;
                var message = new ChatMessage(Author.Model, part.Trim());
                AddMessageToCollection(message);
            }
        }

        private void LoadHistory()
        {
            if (_chatService == null || _chatHistoryManager == null) return;
            var history = _chatHistoryManager.LoadHistory();
            _logger?.LogInfo($"Continuing session with {history.Count} messages from history.");
            foreach (var message in history) { Messages.Add(message); }
            _chatService.LoadHistory(history);
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                MessageAdded?.Invoke();
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        private void NewChat()
        {
            if (_chatService == null || _chatHistoryManager == null || _logger == null) return;
            _logger.LogInfo("--- NEW CHAT SESSION STARTED BY USER ---");
            Messages.Clear();
            _chatService.StartNewChat();
            _chatHistoryManager.SaveHistory(Messages);
        }

        private void OpenSettings()
        {
            _dialogService?.ShowSettingsDialog();
        }
    }
}
