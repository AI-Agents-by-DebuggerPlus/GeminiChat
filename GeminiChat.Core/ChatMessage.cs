namespace GeminiChat.Core
{
    public class ChatMessage
    {
        public Author Author { get; }
        public string Content { get; }
        public DateTime Timestamp { get; }

        public ChatMessage(Author author, string content)
        {
            Author = author;
            Content = content;
            Timestamp = DateTime.UtcNow; // Используем UTC для универсальности
        }
    }
}