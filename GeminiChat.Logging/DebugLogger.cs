using GeminiChat.Core;
using System;
using System.Diagnostics;

namespace GeminiChat.Logging
{
    // Этот класс реализует "контракт" ILogger, который мы создали в Core
    public class DebugLogger : ILogger
    {
        public void LogInfo(string message)
        {
            // Выводим сообщение в окно Output -> Debug в Visual Studio
            Debug.WriteLine($"[INFO] {DateTime.Now:G}: {message}");
        }

        public void LogError(string message, Exception? ex = null)
        {
            Debug.WriteLine($"[ERROR] {DateTime.Now:G}: {message}");
            if (ex != null)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}