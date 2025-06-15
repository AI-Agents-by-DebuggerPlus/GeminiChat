using GeminiChat.Core;
using GenerativeAI;
using GenerativeAI.Types;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public void PrimeHistory(IEnumerable<ChatMessage> history)
        {
            if (history == null || !history.Any()) return;

            _logger.LogInfo($"Priming chat service with {history.Count()} messages from history...");

            _chatSession.History.Clear();
            foreach (var message in history)
            {
                // *** ВОТ ФИНАЛЬНОЕ ИСПРАВЛЕНИЕ ***
                // Мы явно создаем List<Part>, как того требует библиотека,
                // а не массив Part[].
                _chatSession.History.Add(new Content
                {
                    Role = message.Author == Author.User ? "user" : "model",
                    Parts = new List<Part> { new Part { Text = message.Content } }
                });
            }

            _logger.LogInfo("Chat history has been successfully primed.");
        }

        public async Task<string> SendMessageAsync(string message)
        {
            _logger.LogInfo("Sending message to Gemini API with chat history...");
            try
            {
                var response = await _chatSession.GenerateContentAsync(message);
                _logger.LogInfo("Response received from Gemini API.");
                _logger.LogInfo($"Model response: {response.Text}");

                return response.Text;
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while communicating with Gemini API.", ex);
                return "Произошла ошибка при обращении к API Gemini. Подробности в логах.";
            }
        }
    }
}
