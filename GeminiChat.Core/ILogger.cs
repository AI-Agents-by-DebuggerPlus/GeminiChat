using System;

namespace GeminiChat.Core
{
    public interface ILogger
    {
        void LogInfo(string message);
        void LogError(string message, Exception? ex = null);
    }
}