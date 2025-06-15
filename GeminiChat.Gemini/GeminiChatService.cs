using GenerativeAI;
using GeminiChat.Core;
using System;
using System.Threading.Tasks;

namespace GeminiChat.Gemini
{
    public class GeminiChatService : IChatService
    {
        private readonly ILogger _logger;
        private readonly GenerativeModel _model;

        // Конструктор теперь принимает ключ как параметр
        public GeminiChatService(ILogger logger, string apiKey)
        {
            _logger = logger;

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "СЮДА_ВСТАВЬТЕ_ВАШ_API_КЛЮЧ")
            {
                _logger.LogError("API Key is not configured in appsettings.json!", null);
                throw new ArgumentException("API Key is missing. Please configure it in appsettings.json");
            }

            _model = new GenerativeModel(apiKey, model: "gemini-1.5-flash-latest");
            _logger.LogInfo("GeminiChatService (using gunpal5's package) initialized.");
        }

        // Метод SendMessageAsync остается без изменений
        public async Task<string> SendMessageAsync(string message)
        {
            _logger.LogInfo("Sending message to Gemini API...");
            try
            {
                var response = await _model.GenerateContentAsync(message);
                _logger.LogInfo("Response received from Gemini API.");
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