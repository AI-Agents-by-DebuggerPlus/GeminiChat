// ViewModels/MainViewModel.cs
using GeminiChat.Core;
using GeminiChat.Wpf.Commands;
using GeminiChat.Wpf.Messaging;
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
            // <--- ТОЧКА ОСТАНОВА 1: Конструктор MainViewModel
            // Цель: Убедиться, что DI-контейнер успешно создал все сервисы и передал их сюда.
            // Проверьте, что ни один из параметров (chatService, logger и т.д.) не равен null.
            _chatService = chatService;
            _logger = logger;
            _chatHistoryManager = chatHistoryManager;
            _dialogService = dialogService;
            _settingsManager = settingsManager;

            Messages = new ObservableCollection<ChatMessage>();
            SendMessageCommand = new RelayCommand(async _ => await SendMessageAsync(), _ => !string.IsNullOrWhiteSpace(CurrentMessage) && !IsSending);
            NewChatCommand = new RelayCommand(_ => NewChat());
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());

            Messenger.Register<SettingsUpdatedMessage>(OnSettingsUpdated);

            LoadFontSettings();
            LoadHistory();
        }

        private void OnSettingsUpdated(SettingsUpdatedMessage message)
        {
            // <--- ТОЧКА ОСТАНОВА 2: Получение обновления настроек
            // Цель: Проверить, что "Мессенджер" работает и эта ViewModel реагирует на
            // сохранение настроек в другом окне.
            _logger?.LogInfo("Received settings update message. Applying new fonts.");
            LoadFontSettings();
        }

        private void LoadFontSettings()
        {
            if (_settingsManager == null) return;
            // <--- ТОЧКА ОСТАНОВА 3: Загрузка настроек шрифта
            // Цель: Посмотреть, какие именно значения для FontFamily и FontSize
            // загружаются из файла и присваиваются свойствам.
            var settings = _settingsManager.LoadSettings();
            FontFamily = settings.FontFamily;
            FontSize = settings.FontSize;
        }

        private void AddMessageToCollection(ChatMessage message)
        {
            // <--- ТОЧКА ОСТАНОВА 4: Добавление сообщения в UI
            // Цель: Посмотреть на объект 'message' прямо перед его добавлением в коллекцию.
            // Убедитесь, что message.Content содержит ожидаемый текст.
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Messages.Add(message);
                MessageAdded?.Invoke();
            }));
        }

        private async Task SendMessageAsync()
        {
            if (_chatService == null || _chatHistoryManager == null) return;

            // <--- ТОЧКА ОСТАНОВА 5: Начало отправки
            // Цель: Посмотреть, какое именно сообщение от пользователя (`userMessageContent`)
            // будет отправлено в сервис.
            var userMessageContent = CurrentMessage;
            CurrentMessage = string.Empty;
            var userMessage = new ChatMessage(Author.User, userMessageContent);
            AddMessageToCollection(userMessage);
            _chatHistoryManager.SaveHistory(Messages);
            IsSending = true;
            try
            {
                var responseText = await Task.Run(() => _chatService.SendMessageAsync(userMessageContent));

                // <--- ТОЧКА ОСТАНОВА 6: Получен ответ от Gemini
                // Цель: Проанализировать 'responseText'. Это "сырой" ответ от API.
                // Именно здесь мы можем увидеть, содержит ли он ```, текст или и то, и другое.
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
                // <--- ТОЧКА ОСТАНОВА 7: Разделение ответа
                // Цель: Посмотреть на каждую отдельную часть ('part') после разделения.
                // Это поможет понять, почему какой-то из блоков не отображается.
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
            // <--- ТОЧКА ОСТАНОВА 8: Открытие настроек
            // Цель: Убедиться, что команда на открытие настроек вызывается
            // и сервис _dialogService не равен null.
            _dialogService?.ShowSettingsDialog();
        }
    }
}
