using GeminiChat.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace GeminiChat.Wpf.Services
{
    public class ChatHistoryManager
    {
        private readonly string _historyFilePath;
        private readonly ILogger _logger;

        // Теперь сервис принимает логгер для отчетов
        public ChatHistoryManager(ILogger logger)
        {
            _logger = logger;
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolderPath = Path.Combine(appDataPath, "GeminiChatWpf");
            _historyFilePath = Path.Combine(appFolderPath, "chathistory.json");
            _logger.LogInfo($"Chat history path set to: {_historyFilePath}");
        }

        public void SaveHistory(ObservableCollection<ChatMessage> chatHistory)
        {
            try
            {
                string json = JsonConvert.SerializeObject(chatHistory, Newtonsoft.Json.Formatting.Indented);
                Directory.CreateDirectory(Path.GetDirectoryName(_historyFilePath));
                File.WriteAllText(_historyFilePath, json);
                _logger.LogInfo($"Successfully saved {chatHistory.Count} messages to chat history file.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save chat history.", ex);
            }
        }

        public ObservableCollection<ChatMessage> LoadHistory()
        {
            if (!File.Exists(_historyFilePath))
            {
                _logger.LogInfo("Chat history file not found. Starting with a new chat.");
                return new ObservableCollection<ChatMessage>();
            }

            try
            {
                string json = File.ReadAllText(_historyFilePath);
                var history = JsonConvert.DeserializeObject<List<ChatMessage>>(json);

                if (history == null || !history.Any())
                {
                    _logger.LogInfo("Loaded chat history file, but it was empty or invalid.");
                    return new ObservableCollection<ChatMessage>();
                }

                _logger.LogInfo($"Successfully loaded {history.Count} messages from chat history file.");
                return new ObservableCollection<ChatMessage>(history);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load or parse chat history file.", ex);
                return new ObservableCollection<ChatMessage>();
            }
        }
    }
}
