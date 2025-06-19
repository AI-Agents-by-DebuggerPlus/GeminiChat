// ViewModels/MainViewModel.cs
using GeminiChat.Core;
using GeminiChat.Wpf.Commands;
using GeminiChat.Wpf.Messaging;
using GeminiChat.Wpf.Services;
using Microsoft.Win32; // Для OpenFileDialog
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging; // Для BitmapImage

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

        // --- Поля для свойств ---
        private string _currentMessage = "";
        private bool _isSending;
        private string _fontFamily = "Segoe UI";
        private double _fontSize = 15;

        // --- НОВЫЕ ПОЛЯ ДЛЯ ПРИКРЕПЛЕНИЯ ИЗОБРАЖЕНИЯ ---
        private BitmapImage? _attachedImagePreview;
        private byte[]? _attachedImageData;
        private string? _attachedImageMimeType;

        // --- Свойства для привязки к UI ---
        public ObservableCollection<ChatMessage> Messages { get; }
        public string CurrentMessage { get => _currentMessage; set { if (SetProperty(ref _currentMessage, value)) RaiseCanExecuteChanged(); } }
        public bool IsSending { get => _isSending; set { if (SetProperty(ref _isSending, value)) RaiseCanExecuteChanged(); } }
        public string FontFamily { get => _fontFamily; set => SetProperty(ref _fontFamily, value); }
        public double FontSize { get => _fontSize; set => SetProperty(ref _fontSize, value); }

        public BitmapImage? AttachedImagePreview
        {
            get => _attachedImagePreview;
            set { if (SetProperty(ref _attachedImagePreview, value)) RaiseCanExecuteChanged(); }
        }

        // --- Команды ---
        public ICommand SendMessageCommand { get; }
        public ICommand NewChatCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand SendSystemInstructionCommand { get; }
        public ICommand AttachImageCommand { get; }
        public ICommand RemoveImageCommand { get; }

        public event Action? MessageAdded;

        // Конструктор для дизайнера
        public MainViewModel()
        {
            Messages = new ObservableCollection<ChatMessage>();
            SendMessageCommand = new RelayCommand(_ => { }, _ => false);
            NewChatCommand = new RelayCommand(_ => { });
            OpenSettingsCommand = new RelayCommand(_ => { });
            SendSystemInstructionCommand = new RelayCommand(_ => { });
            AttachImageCommand = new RelayCommand(_ => { });
            RemoveImageCommand = new RelayCommand(_ => { });
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

            SendMessageCommand = new RelayCommand(async _ => await SendMessageAsync(), _ => CanSendMessage());
            NewChatCommand = new RelayCommand(_ => NewChat());
            OpenSettingsCommand = new RelayCommand(_ => OpenSettings());
            SendSystemInstructionCommand = new RelayCommand(_ => SendSystemInstruction());
            AttachImageCommand = new RelayCommand(_ => AttachImage());
            RemoveImageCommand = new RelayCommand(_ => RemoveImage());

            Messenger.Register<SettingsUpdatedMessage>(OnSettingsUpdated);
            LoadFontSettings();
            LoadHistory();
        }

        private void RaiseCanExecuteChanged()
        {
            (SendMessageCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private bool CanSendMessage()
        {
            // Отправлять можно, если есть либо текст, либо картинка, и не идет отправка
            return (!string.IsNullOrWhiteSpace(CurrentMessage) || _attachedImageData != null) && !IsSending;
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
            var imageData = _attachedImageData;
            var imageMimeType = _attachedImageMimeType;

            CurrentMessage = string.Empty;
            RemoveImage();

            ChatMessage userMessage;
            if (imageData != null && imageMimeType != null)
            {
                userMessage = new ChatMessage(Author.User, userMessageContent, imageData, imageMimeType);
            }
            else
            {
                userMessage = new ChatMessage(Author.User, userMessageContent);
            }

            AddMessageToCollection(userMessage);

            IsSending = true;
            try
            {
                var responseText = await Task.Run(() => _chatService.SendMessageAsync(userMessageContent, imageData, imageMimeType));

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

        private void SendSystemInstruction()
        {
            if (_chatService == null || _logger == null) return;
            try
            {
                string instruction = File.ReadAllText("ExecutorSystemPrompt.txt");
                _chatService.SetSystemInstruction(instruction);
                var infoMessage = new ChatMessage(Author.System, "Инструкция для Executor отправлена.");
                AddMessageToCollection(infoMessage);
                _logger.LogInfo("[ViewModel] System instruction sent to ChatService.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to read system instruction from file.", ex);
                var errorMessage = new ChatMessage(Author.System, "Ошибка: не удалось загрузить инструкцию.");
                AddMessageToCollection(errorMessage);
            }
        }

        private void AttachImage()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Images (*.png;*.jpeg;*.jpg;*.webp)|*.png;*.jpeg;*.jpg;*.webp",
                Title = "Выберите изображение"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _attachedImageData = File.ReadAllBytes(openFileDialog.FileName);
                    _attachedImageMimeType = GetMimeType(openFileDialog.FileName);

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(openFileDialog.FileName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    AttachedImagePreview = bitmap;

                    _logger?.LogInfo($"Image attached: {openFileDialog.FileName}");
                }
                catch (Exception ex)
                {
                    _logger?.LogError("Failed to attach image.", ex);
                    MessageBox.Show("Не удалось прикрепить изображение.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RemoveImage()
        {
            AttachedImagePreview = null;
            _attachedImageData = null;
            _attachedImageMimeType = null;
        }

        private string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}
