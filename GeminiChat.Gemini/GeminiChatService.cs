using GenerativeAI;
using GeminiChat.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; // <-- Добавляем для StringBuilder
using System.Threading.Tasks;

namespace GeminiChat.Gemini
{
    public class GeminiChatService : IChatService
    {
        private readonly ILogger _logger;
        private readonly GenerativeModel _model;
        private ChatSession _chatSession;

        public GeminiChatService(ILogger logger, string apiKey)
        {
            _logger = logger;

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.StartsWith("СЮДА"))
            {
                var errorMessage = "API Key is not configured correctly in appsettings.json!";
                _logger.LogError(errorMessage, null);
                throw new ArgumentException(errorMessage);
            }

            _model = new GenerativeModel(apiKey, model: "gemini-1.5-flash-latest");
            _chatSession = _model.StartChat();
            _logger.LogInfo("GeminiChatService initialized.");
        }

        public async Task PrimeContextAsync(IEnumerable<ChatMessage> history)
        {
            _logger.LogInfo("--- Starting Context Priming For New Session ---");
            _chatSession = _model.StartChat(); // Всегда начинаем с новой, чистой сессии

            if (history == null || !history.Any())
            {
                _logger.LogInfo("Chat history is empty. No context to prime.");
                _logger.LogInfo("--- Context Priming Finished ---");
                return;
            }

            // *** НАША НОВАЯ РАБОЧАЯ ЛОГИКА ***
            // 1. Собираем всю историю в одну большую строку.
            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine("СИСТЕМНАЯ ИНСТРУКЦИЯ: Это предыдущая история диалога. Запомни ее и используй для контекста в своих ответах. Не нужно пересказывать эту историю пользователю, просто знай ее.");
            contextBuilder.AppendLine("--- НАЧАЛО ИСТОРИИ ---");
            foreach (var message in history)
            {
                contextBuilder.AppendLine($"{message.Author}: {message.Content}");
            }
            contextBuilder.AppendLine("--- КОНЕЦ ИСТОРИИ ---");
            contextBuilder.AppendLine("Ответь 'OK', чтобы подтвердить, что контекст загружен.");

            string contextPrompt = contextBuilder.ToString();

            _logger.LogInfo($"Injecting context prompt ({history.Count()} messages)...");

            try
            {
                // 2. Отправляем эту строку как "нулевое" сообщение в новой сессии.
                // Модель ответит "ОК", и этот диалог сохранится в _chatSession,
                // тем самым "запомнив" всю переданную историю.
                var response = await _chatSession.GenerateContentAsync(contextPrompt);
                _logger.LogInfo($"SUCCESS: Context priming message sent. Model responded with: '{response.Text}'");
            }
            catch (Exception ex)
            {
                _logger.LogError("CRITICAL: An exception occurred while priming the context.", ex);
            }
            _logger.LogInfo("--- Context Priming Finished ---");
        }

        public async Task<string> SendMessageAsync(string message)
        {
            _logger.LogInfo($"Sending message. Current session history count: {_chatSession.History.Count}");
            try
            {
                var response = await _chatSession.GenerateContentAsync(message);
                _logger.LogInfo($"Response received. Session history count is now: {_chatSession.History.Count}");
                _logger.LogInfo($"Model response: {response.Text}");
                return response.Text;
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while sending a message.", ex);
                return "Произошла ошибка при обращении к API Gemini. Подробности в логах.";
            }
        }
    }
}
