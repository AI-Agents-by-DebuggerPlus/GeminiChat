// Core/ChatMessage.cs
using System.Text.Json.Serialization;

namespace GeminiChat.Core
{
    public class ChatMessage
    {
        public Author Author { get; set; }
        public string Content { get; set; }

        // --- НОВЫЕ СВОЙСТВА ---
        /// <summary>
        /// Содержит бинарные данные изображения, если оно прикреплено.
        /// Сериализатор JSON автоматически преобразует это в строку base64.
        /// </summary>
        public byte[]? ImageData { get; set; }

        /// <summary>
        /// MIME-тип изображения (например, "image/png" или "image/jpeg").
        /// </summary>
        public string? ImageMimeType { get; set; }

        // --- КОНСТРУКТОРЫ ---

        // Пустой конструктор необходим для корректной десериализации из JSON.
        public ChatMessage()
        {
            Content = string.Empty;
        }

        // Существующий конструктор для простых текстовых сообщений.
        public ChatMessage(Author author, string content)
        {
            Author = author;
            Content = content;
        }

        /// <summary>
        /// Новый конструктор для сообщений с изображением.
        /// </summary>
        public ChatMessage(Author author, string content, byte[] imageData, string imageMimeType)
        {
            Author = author;
            Content = content;
            ImageData = imageData;
            ImageMimeType = imageMimeType;
        }
    }
}
