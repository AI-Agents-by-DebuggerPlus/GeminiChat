// ViewModels/MainViewModel.cs
using GeminiChat.Core;
using GeminiChat.Wpf.Commands;
using GeminiChat.Wpf.Messaging;
using GeminiChat.Wpf.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
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
        private readonly ICommandInterpreter? _commandInterpreter;

        // --- Поля и свойства ---
        private string _currentMessage = "";
        private bool _isSending;
        private string _fontFamily = "Segoe UI";
        private double _fontSize = 15;
        public ObservableCollection<ChatMessage> Messages { get; }
        public string CurrentMessage { get => _currentMessage; set => SetProperty(ref _currentMessage, value); }
        public bool IsSending { get => _isSending; set => SetProperty(ref _isSending, value); }
        public string FontFamily { get => _fontFamily; set => SetProperty(ref _fontFamily, value); }
        public double FontSize { get => _fontSize; set => SetProperty(ref _fontSize, value); }

        // --- Команды ---
        public ICommand SendMessageCommand { get; }
        public ICommand NewChatCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand SendSystemInstructionCommand { get; }

        public event Action? MessageAdded;

        // Конструктор для дизайнера
        public MainViewModel()
        {
            Messages = new ObservableCollection<ChatMessage>();
            SendMessageCommand = new RelayCommand(_ => { }, _ => false);
            NewChatCommand = new RelayCommand(_ => { });
            OpenSettingsCommand = new RelayCommand(_ => { });
            SendSystemInstructionCommand = new RelayCommand(_ => { });
        }

        // Основной конструктор
        public MainViewModel(IChatService chatService, ILogger logger, ChatHistoryManager chatHistoryManager, IDialogService dialogService, SettingsManager settingsManager, ICommandInterpreter commandInterpreter)
        {
            _chatService = chatService;
            _logger = logger;
            _chatHistoryManager = chatHistoryManager;
            _dialogService = dialogService;
            _settingsManager = settingsManager;
            _commandInterpreter = commandInterpreter;

            Messages = new ObservableCollection<ChatMessage>();
            Messages.CollectionChanged += OnMessagesChanged;
            SendMessageCommand = new RelayCommand(async _ => await SendMessageAsync(), _ => !string.IsNullOrWhiteSpace(CurrentMessage) && !IsSending);
            NewChatCommand = new RelayCommand(_ => NewChat());
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
            SendSystemInstructionCommand = new RelayCommand(_ => SendSystemInstruction());

            Messenger.Register<SettingsUpdatedMessage>(OnSettingsUpdated);
            LoadFontSettings();
            LoadHistory();
        }

        private void SendSystemInstruction()
        {
            if (_chatService == null || _logger == null) return;

            try
            {
                // Читаем инструкцию из файла
                string instruction = File.ReadAllText("ExecutorSystemPrompt.txt");
                _chatService.SetSystemInstruction(instruction);

                var infoMessage = new ChatMessage(Author.System, "Инструкция для Executor отправлена. Теперь вы можете давать команды в произвольной форме.");
                AddMessageToCollection(infoMessage);
                _logger.LogInfo("[ViewModel] System instruction loaded from file and sent to ChatService.");
            }
            catch (Exception ex)
            {
                // Обрабатываем возможные ошибки (например, файл не найден)
                _logger.LogError("Failed to read system instruction from file.", ex);
                var errorMessage = new ChatMessage(Author.System, "Ошибка: не удалось загрузить системную инструкцию.");
                AddMessageToCollection(errorMessage);
            }
        }

        private void OnMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e) => _chatHistoryManager?.SaveHistory(Messages);
        private void OnSettingsUpdated(SettingsUpdatedMessage message) => LoadFontSettings();
        private void LoadFontSettings()
        {
            if (_settingsManager == null) return;
            var settings = _settingsManager.LoadSettings();
            FontFamily = settings.FontFamily;
            FontSize = settings.FontSize;
        }
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
            if (_chatService == null) return;
            var userMessageContent = CurrentMessage;
            CurrentMessage = string.Empty;
            var userMessage = new ChatMessage(Author.User, userMessageContent);
            AddMessageToCollection(userMessage);

            IsSending = true;
            try
            {
                var responseText = await Task.Run(() => _chatService.SendMessageAsync(userMessageContent));
                ProcessAndDisplayResponse(responseText);
                if (_commandInterpreter != null)
                {
                    await _commandInterpreter.InterpretAndExecuteAsync(responseText);
                }
            }
            finally { IsSending = false; }
        }
        private void ProcessAndDisplayResponse(string response)
        {
            var codeBlockPattern = @"(```[\s\S]*?```)";
            var commandPattern = @"(%%COMMAND_JSON%%[\s\S]*)";
            var textOnly = Regex.Replace(response, commandPattern, "");
            var parts = Regex.Split(textOnly, codeBlockPattern);
            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part)) continue;
                var message = new ChatMessage(Author.Model, part.Trim());
                AddMessageToCollection(message);
            }
        }
        private void LoadHistory()
        {
            if (_chatService == null || _chatHistoryManager == null || _logger == null) return;
            var history = _chatHistoryManager.LoadHistory();
            _logger.LogInfo($"--- CONTINUING SESSION WITH {history.Count} MESSAGES ---");
            foreach (var message in history) { Messages.Add(message); }
            _chatService.LoadHistory(history);
            Application.Current.Dispatcher.BeginInvoke(new Action(() => { MessageAdded?.Invoke(); }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }
        private void NewChat()
        {
            if (_chatService == null || _logger == null) return;
            _logger.LogInfo("--- NEW CHAT SESSION STARTED BY USER ---");
            Messages.Clear();
            _chatService.StartNewChat();
        }
        private void OpenSettings() => _dialogService?.ShowSettingsDialog();
    }
}
