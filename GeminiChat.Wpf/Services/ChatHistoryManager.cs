using GeminiChat.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace GeminiChat.Wpf.Services
{
    public class ChatHistoryManager
    {
        private readonly string _historyFilePath;

        public ChatHistoryManager()
        {
            // Используем тот же подход, что и для настроек, но с другим именем файла
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolderPath = Path.Combine(appDataPath, "GeminiChatWpf");
            _historyFilePath = Path.Combine(appFolderPath, "chathistory.json");
        }

        public void SaveHistory(ObservableCollection<ChatMessage> chatHistory)
        {
            try
            {
                string json = JsonConvert.SerializeObject(chatHistory, Newtonsoft.Json.Formatting.Indented);
                Directory.CreateDirectory(Path.GetDirectoryName(_historyFilePath));
                File.WriteAllText(_historyFilePath, json);
            }
            catch (Exception)
            {
                // В реальном приложении здесь обязательно должно быть логирование ошибки
            }
        }

        public ObservableCollection<ChatMessage> LoadHistory()
        {
            if (!File.Exists(_historyFilePath))
            {
                // Если файла нет, возвращаем пустую коллекцию
                return new ObservableCollection<ChatMessage>();
            }

            try
            {
                string json = File.ReadAllText(_historyFilePath);
                var history = JsonConvert.DeserializeObject<List<ChatMessage>>(json);

                // Преобразуем List<T> в ObservableCollection<T>
                return history != null ? new ObservableCollection<ChatMessage>(history) : new ObservableCollection<ChatMessage>();
            }
            catch (Exception)
            {
                // Если файл поврежден, тоже возвращаем пустую коллекцию
                return new ObservableCollection<ChatMessage>();
            }
        }
    }
}
