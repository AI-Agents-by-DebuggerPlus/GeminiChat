// Messaging/Messenger.cs
using System;

namespace GeminiChat.Wpf.Messaging
{
    /// <summary>
    /// Простой статический класс для обмена сообщениями между компонентами.
    /// </summary>
    public static class Messenger
    {
        private static readonly Dictionary<Type, List<Action<object>>> _recipients = new();

        /// <summary>
        /// Подписка на получение сообщений определенного типа.
        /// </summary>
        public static void Register<T>(Action<T> action) where T : class
        {
            var messageType = typeof(T);
            if (!_recipients.ContainsKey(messageType))
            {
                _recipients[messageType] = new List<Action<object>>();
            }
            _recipients[messageType].Add(message => action(message as T));
        }

        /// <summary>
        /// Отправка сообщения всем подписчикам.
        /// </summary>
        public static void Send<T>(T message) where T : class
        {
            var messageType = typeof(T);
            if (_recipients.ContainsKey(messageType))
            {
                foreach (var action in _recipients[messageType])
                {
                    action(message);
                }
            }
        }
    }

    /// <summary>
    /// Класс-сообщение, сигнализирующий об обновлении настроек.
    /// </summary>
    public class SettingsUpdatedMessage { }
}
