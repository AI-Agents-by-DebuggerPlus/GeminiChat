// Core/Author.cs
namespace GeminiChat.Core
{
    /// <summary>
    /// Определяет автора сообщения в чате.
    /// </summary>
    public enum Author
    {
        /// <summary>
        /// Сообщение от пользователя.
        /// </summary>
        User,

        /// <summary>
        /// Сообщение от языковой модели.
        /// </summary>
        Model,

        /// <summary>
        /// Информационное сообщение от самого приложения.
        /// </summary>
        System
    }
}
