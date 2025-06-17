// Logging/FileLogger.cs
using GeminiChat.Core;
using System;
using System.IO;
using System.Text;

namespace GeminiChat.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _logFilePath;
        private readonly object _lock = new object();

        public FileLogger(string logDirectory)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            _logFilePath = Path.Combine(logDirectory, "session.log");

            // ИСПРАВЛЕНО: Добавляем разделитель для новой сессии при запуске
            try
            {
                lock (_lock)
                {
                    File.AppendAllText(_logFilePath,
                        $"{Environment.NewLine}--- NEW SESSION STARTED AT {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---{Environment.NewLine}{Environment.NewLine}");
                }
            }
            catch { /* Игнорируем */ }
        }

        private void Log(string message, string level)
        {
            try
            {
                lock (_lock)
                {
                    var formattedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";
                    File.AppendAllText(_logFilePath, formattedMessage);
                }
            }
            catch { /* Игнорируем ошибки логирования */ }
        }

        public void LogInfo(string message) => Log(message, "INFO");
        public void LogWarning(string message) => Log(message, "WARN");
        public void LogError(string message) => Log(message, "ERROR");

        public void LogError(string message, Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine($"Exception: {ex.GetType().FullName}");
            sb.AppendLine($"Message: {ex.Message}");
            sb.AppendLine($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                sb.AppendLine("--- Inner Exception ---");
                sb.AppendLine($"Exception: {ex.InnerException.GetType().FullName}");
                sb.AppendLine($"Message: {ex.InnerException.Message}");
                sb.AppendLine($"StackTrace: {ex.InnerException.StackTrace}");
            }
            Log(sb.ToString(), "ERROR");
        }
    }
}
