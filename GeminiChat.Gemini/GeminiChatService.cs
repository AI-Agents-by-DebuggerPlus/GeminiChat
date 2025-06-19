// Gemini/GeminiChatService.cs
using GeminiChat.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GeminiChat.Gemini
{
    public class GeminiChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger _chatLogger;
        private List<GeminiRequestContent> _history = new List<GeminiRequestContent>();
        private string? _systemInstruction = null;

        public GeminiChatService(string apiKey, ILogger chatLogger)
        {
            _apiKey = apiKey;
            _chatLogger = chatLogger;
            _httpClient = new HttpClient { BaseAddress = new Uri("https://generativelanguage.googleapis.com/") };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // --- ВОЗВРАЩАЕМ НЕДОСТАЮЩИЙ МЕТОД ---
        public void SetSystemInstruction(string instruction)
        {
            _systemInstruction = instruction;
            _chatLogger.LogInfo("[Service] System instruction has been set.");
        }

        public async Task<string> SendMessageAsync(string message)
        {
            try
            {
                // Логика добавления системной инструкции к сообщению
                if (!string.IsNullOrEmpty(_systemInstruction))
                {
                    message = $"{_systemInstruction}\n\nUser: {message}";
                    _chatLogger.LogInfo("[Service] Prepending system instruction to the message.");
                    _systemInstruction = null;
                }

                _chatLogger.LogInfo($"--> USER: {message}");

                if (!string.IsNullOrWhiteSpace(message))
                {
                    _history.Add(new GeminiRequestContent { Role = "user", Parts = new[] { new Part { Text = message } } });
                }

                var validHistory = _history.Where(h => h.Parts != null && h.Parts.Any(p => !string.IsNullOrWhiteSpace(p.Text))).ToList();
                if (!validHistory.Any()) return "Please provide a message.";

                var requestPayload = new GeminiRequest { Contents = validHistory.ToArray() };

                var serializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var jsonPayload = JsonSerializer.Serialize(requestPayload, serializerOptions);
                var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var requestUrl = $"/v1beta/models/gemini-1.5-flash-latest:generateContent?key={_apiKey}";
                var response = await _httpClient.PostAsync(requestUrl, httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _chatLogger.LogError($"API Error: {response.StatusCode} - {errorContent}");
                    return "Error communicating with the Gemini API.";
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(jsonResponse, serializerOptions);
                var responseText = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "No content received.";

                _chatLogger.LogInfo($"<-- MODEL: {responseText}");
                _history.Add(new GeminiRequestContent { Role = "model", Parts = new[] { new Part { Text = responseText } } });
                return responseText;
            }
            catch (Exception ex)
            {
                _chatLogger.LogError("Exception in SendMessageAsync", ex);
                return "An unexpected error occurred.";
            }
        }

        public void StartNewChat()
        {
            _history.Clear();
            _chatLogger.LogInfo("--- NEW CHAT SESSION ---");
        }

        public void LoadHistory(IEnumerable<Core.ChatMessage> history)
        {
            _history = history.Where(m => !string.IsNullOrWhiteSpace(m.Content))
                              .Select(m => new GeminiRequestContent { Role = m.Author == Core.Author.User ? "user" : "model", Parts = new[] { new Part { Text = m.Content } } })
                              .ToList();
            if (history.Any())
                _chatLogger.LogInfo($"--- CONTINUING SESSION WITH {_history.Count} MESSAGES ---");
        }
    }

    #region Helper Classes
    internal class GeminiRequest
    {
        public GeminiRequestContent[] Contents { get; set; } = default!;
    }
    internal class GeminiRequestContent
    {
        public string Role { get; set; } = default!;
        public Part[] Parts { get; set; } = default!;
    }
    internal class Part
    {
        public string Text { get; set; } = default!;
    }
    internal class GeminiResponse
    {
        public Candidate[]? Candidates { get; set; }
        public PromptFeedback? PromptFeedback { get; set; }
    }
    internal class Candidate
    {
        public GeminiRequestContent? Content { get; set; }
    }
    internal class PromptFeedback
    {
        public string? BlockReason { get; set; }
    }
    #endregion
}
