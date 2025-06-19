// Core/IChatService.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeminiChat.Core
{
    public interface IChatService
    {
        /// <summary>
        /// Отправляет сообщение (и опционально изображение) в сервис Gemini.
        /// </summary>
        /// <param name="message">Текстовое сообщение.</param>
        /// <param name="imageData">Бинарные данные изображения (может быть null).</param>
        /// <param name="mimeType">MIME-тип изображения (может быть null).</param>
        /// <returns>Текстовый ответ от модели.</returns>
        Task<string> SendMessageAsync(string message, byte[]? imageData = null, string? mimeType = null);

        void StartNewChat();

        void LoadHistory(IEnumerable<ChatMessage> history);

        void SetSystemInstruction(string instruction);
    }
}