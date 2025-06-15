using GeminiChat.Core;
using GeminiChat.Wpf.Commands;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GeminiChat.Wpf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly IChatService _chatService; // <-- Добавили сервис чата
        private string _userInput = string.Empty;
        private bool _isThinking; // <-- Добавили флаг "загрузки"

        public ObservableCollection<ChatMessage> ChatHistory { get; } = new();

        public string UserInput
        {
            get => _userInput;
            set
            {
                _userInput = value;
                OnPropertyChanged();
            }
        }

        // Свойство для UI, чтобы понимать, когда идет запрос к API
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

        // В конструктор теперь приходит два сервиса: ILogger и IChatService
        public MainViewModel(ILogger logger, IChatService chatService)
        {
            _logger = logger;
            _chatService = chatService; // <-- Сохраняем сервис
            _logger.LogInfo("MainViewModel created and services injected.");

            SendCommand = new RelayCommand(
                execute: async _ => await OnSend(), // <-- Делаем команду асинхронной
                canExecute: _ => !string.IsNullOrWhiteSpace(UserInput) && !IsThinking // <-- Блокируем кнопку во время ответа
            );
        }

        private async Task OnSend()
        {
            string messageToSend = UserInput;

            // Добавляем сообщение пользователя в историю и сразу очищаем поле ввода
            ChatHistory.Add(new ChatMessage(Author.User, messageToSend));
            UserInput = string.Empty;

            IsThinking = true; // <-- Включаем индикатор загрузки
            _logger.LogInfo($"User message to be sent: {messageToSend}");

            try
            {
                // Вызываем наш сервис, который общается с Gemini
                var responseText = await _chatService.SendMessageAsync(messageToSend);

                // Добавляем ответ от модели в историю
                ChatHistory.Add(new ChatMessage(Author.Model, responseText));
            }
            catch (System.Exception ex)
            {
                _logger.LogError("Failed to get response from chat service.", ex);
                // В случае ошибки, тоже добавляем сообщение в чат
                ChatHistory.Add(new ChatMessage(Author.Model, "Извините, произошла ошибка."));
            }
            finally
            {
                IsThinking = false; // <-- В любом случае выключаем индикатор загрузки
            }
        }
    }
}