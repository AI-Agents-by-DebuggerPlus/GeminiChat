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
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _chatLogger = chatLogger ?? throw new ArgumentNullException(nameof(chatLogger));
            _httpClient = new HttpClient { BaseAddress = new Uri("https://generativelanguage.googleapis.com/") };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void SetSystemInstruction(string instruction)
        {
            _systemInstruction = instruction;
        }

        public async Task<string> SendMessageAsync(string message, byte[]? imageData = null, string? mimeType = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(_systemInstruction))
                {
                    message = $"{_systemInstruction}\n\nUser: {message}";
                    _systemInstruction = null;
                }

                _chatLogger.LogInfo($"--> USER: {message}");
                if (imageData != null) _chatLogger.LogInfo($"--> ATTACHMENT: {mimeType}, {imageData.Length} bytes");

                var parts = new List<Part>();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    parts.Add(new Part { Text = message });
                }
                if (imageData != null && mimeType != null)
                {
                    parts.Add(new Part { InlineData = new InlineData { MimeType = mimeType, Data = Convert.ToBase64String(imageData) } });
                }

                if (parts.Any())
                {
                    _history.Add(new GeminiRequestContent { Role = "user", Parts = parts.ToArray() });
                }

                var validHistory = _history.Where(h => h.Parts != null && h.Parts.Any()).ToList();
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
            _history.Clear();
            foreach (var message in history)
            {
                var parts = new List<Part>();
                if (!string.IsNullOrWhiteSpace(message.Content))
                {
                    parts.Add(new Part { Text = message.Content });
                }
                if (message.ImageData != null && message.ImageMimeType != null)
                {
                    parts.Add(new Part { InlineData = new InlineData { MimeType = message.ImageMimeType, Data = Convert.ToBase64String(message.ImageData) } });
                }
                if (parts.Any())
                {
                    _history.Add(new GeminiRequestContent
                    {
                        Role = message.Author == Core.Author.User ? "user" : "model",
                        Parts = parts.ToArray()
                    });
                }
            }
            if (history.Any())
                _chatLogger.LogInfo($"--- CONTINUING SESSION WITH {history.Count()} MESSAGES ---");
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
        public string? Text { get; set; }
        public InlineData? InlineData { get; set; }
    }
    internal class InlineData
    {
        public string MimeType { get; set; } = default!;
        public string Data { get; set; } = default!;
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