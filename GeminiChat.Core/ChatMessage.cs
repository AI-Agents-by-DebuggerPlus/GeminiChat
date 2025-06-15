using Newtonsoft.Json;
using System;

namespace GeminiChat.Core
{
    public class ChatMessage
    {
        public Author Author { get; }
        public string Content { get; }
        public DateTime Timestamp { get; }

        // Этот конструктор мы используем в приложении
        public ChatMessage(Author author, string content)
        {
            Author = author;
            Content = content;
            Timestamp = DateTime.UtcNow;
        }

        // Этот конструктор будет использовать Newtonsoft.Json для восстановления объекта из файла.
        // *** ИСПРАВЛЕНИЕ ЗДЕСЬ ***
        // Мы указываем полное имя, чтобы компилятор не путался.
        [Newtonsoft.Json.JsonConstructor]
        public ChatMessage(Author author, string content, DateTime timestamp)
        {
            Author = author;
            Content = content;
            Timestamp = timestamp;
        }
    }
}
