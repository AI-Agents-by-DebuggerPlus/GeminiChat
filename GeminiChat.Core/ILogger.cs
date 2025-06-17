// Core/ILogger.cs
using System;

namespace GeminiChat.Core
{
    public interface ILogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
        // Добавляем перегрузку для логирования полного исключения
        void LogError(string message, Exception ex);
    }
}
