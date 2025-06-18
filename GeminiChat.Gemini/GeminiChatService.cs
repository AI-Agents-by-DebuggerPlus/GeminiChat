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

        public GeminiChatService(string apiKey, ILogger chatLogger)
        {
            _apiKey = apiKey;
            _chatLogger = chatLogger;
            _httpClient = new HttpClient { BaseAddress = new Uri("https://generativelanguage.googleapis.com/") };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> SendMessageAsync(string message)
        {
            try
            {
                _chatLogger.LogInfo($"--> USER: {message}");
                if (!string.IsNullOrWhiteSpace(message))
                {
                    _history.Add(new GeminiRequestContent { Role = "user", Parts = new[] { new Part { Text = message } } });
                }

                var validHistory = _history.Where(h => h.Parts != null && h.Parts.Any(p => !string.IsNullOrWhiteSpace(p.Text))).ToList();
                if (!validHistory.Any()) return "Please provide a message.";

                var requestPayload = new GeminiRequest { Contents = validHistory.ToArray() };
                var jsonPayload = JsonSerializer.Serialize(requestPayload);
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
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(jsonResponse);
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
            _chatLogger.LogInfo($"--- CONTINUING SESSION WITH {_history.Count} MESSAGES ---");
        }
    }

    #region Helper Classes for JSON Serialization

    // ИСПРАВЛЕНО: Все свойства теперь public
    internal class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public GeminiRequestContent[] Contents { get; set; } = default!;
    }

    internal class GeminiRequestContent
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = default!;

        [JsonPropertyName("parts")]
        public Part[] Parts { get; set; } = default!;
    }

    internal class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = default!;
    }

    internal class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[]? Candidates { get; set; }
    }

    internal class Candidate
    {
        [JsonPropertyName("content")]
        public GeminiRequestContent? Content { get; set; }
    }

    #endregion
}
