// ViewModels/MainViewModel.cs
using GeminiChat.Core;
using GeminiChat.Wpf.Commands;
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
        private readonly IChatService _chatService;
        private readonly ILogger _logger;
        private readonly ChatHistoryManager _chatHistoryManager;
        private string _currentMessage = "";
        private bool _isSending;

        public ObservableCollection<ChatMessage> Messages { get; }
        public string CurrentMessage
        {
            get => _currentMessage;
            set
            {
                if (SetProperty(ref _currentMessage, value))
                {
                    ((RelayCommand)SendMessageCommand).RaiseCanExecuteChanged();
                }
            }
        }
        public bool IsSending
        {
            get => _isSending;
            set
            {
                if (SetProperty(ref _isSending, value))
                {
                    ((RelayCommand)SendMessageCommand).RaiseCanExecuteChanged();
                }
            }
        }
        public ICommand SendMessageCommand { get; }
        public ICommand NewChatCommand { get; }

        public event Action? MessageAdded;

        // Конструктор за дизайнера
        public MainViewModel()
        {
            Messages = new ObservableCollection<ChatMessage>();
            SendMessageCommand = new RelayCommand(_ => { }, _ => false);
            NewChatCommand = new RelayCommand(_ => { });
        }

        public MainViewModel(IChatService chatService, ILogger logger, ChatHistoryManager chatHistoryManager)
        {
            _chatService = chatService;
            _logger = logger;
            _chatHistoryManager = chatHistoryManager;
            Messages = new ObservableCollection<ChatMessage>();
            SendMessageCommand = new RelayCommand(async _ => await SendMessageAsync(), _ => CanSendMessage());
            NewChatCommand = new RelayCommand(_ => NewChat());
            LoadHistory();
        }

        private bool CanSendMessage() => !string.IsNullOrWhiteSpace(CurrentMessage) && !IsSending;

        private void AddMessageToCollection(ChatMessage message)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _logger.LogInfo($"[UI] Adding message from '{message.Author}' to UI collection.");
                Messages.Add(message);
                _logger.LogInfo($"[UI] Message added. Collection count is now: {Messages.Count}");
                MessageAdded?.Invoke();
            }));
        }

        private async Task SendMessageAsync()
        {
            var userMessageContent = CurrentMessage;
            CurrentMessage = string.Empty;
            var userMessage = new ChatMessage(Author.User, userMessageContent);
            AddMessageToCollection(userMessage);
            _chatHistoryManager.SaveHistory(Messages);

            IsSending = true;
            try
            {
                var responseText = await Task.Run(() => _chatService.SendMessageAsync(userMessageContent));

                // --- НОВА ЛОГИКА ЗА РАЗДЕЛЯНЕ НА СЪОБЩЕНИЯ ---
                ProcessAndDisplayResponse(responseText);

                // Запазваме историята след като всички части са добавени
                _chatHistoryManager.SaveHistory(Messages);
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred during SendMessageAsync.", ex);
                AddMessageToCollection(new ChatMessage(Author.Model, "Sorry, an error occurred."));
            }
            finally
            {
                IsSending = false;
            }
        }

        /// <summary>
        /// НОВ МЕТОД: Анализира отговора и го разделя на части (текст/код).
        /// </summary>
        private void ProcessAndDisplayResponse(string response)
        {
            // Регулярен израз, който намира всички блокове, започващи и завършващи с ```
            var codeBlockPattern = @"(```[\s\S]*?```)";
            var parts = Regex.Split(response, codeBlockPattern);

            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                {
                    continue;
                }

                // Създаваме ново съобщение за всяка част
                var message = new ChatMessage(Author.Model, part.Trim());
                AddMessageToCollection(message);
            }
        }

        private void LoadHistory()
        {
            var history = _chatHistoryManager.LoadHistory();
            _logger.LogInfo($"Continuing session with {history.Count} messages from history.");
            foreach (var message in history)
            {
                Messages.Add(message);
            }
            _chatService.LoadHistory(history);
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                MessageAdded?.Invoke();
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        private void NewChat()
        {
            _logger.LogInfo("--- NEW CHAT SESSION STARTED BY USER ---");
            Messages.Clear();
            _chatService.StartNewChat();
            _chatHistoryManager.SaveHistory(Messages);
        }
    }
}
