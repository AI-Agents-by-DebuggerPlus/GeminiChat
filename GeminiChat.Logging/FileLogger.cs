using GeminiChat.Core;
using System;
using System.IO;

namespace GeminiChat.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _logFilePath;
        private static readonly object _lock = new object();

        public FileLogger()
        {
            // Создаем путь к папке логов
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string logFolderPath = Path.Combine(appDataPath, "GeminiChatWpf", "Logs");

            Directory.CreateDirectory(logFolderPath);

            // Теперь будем использовать один и тот же файл для каждой сессии
            _logFilePath = Path.Combine(logFolderPath, "session.log");

            // *** ИЗМЕНЕНИЕ ЗДЕСЬ ***
            // Очищаем старый лог-файл при каждом запуске приложения
            try
            {
                if (File.Exists(_logFilePath))
                {
                    File.Delete(_logFilePath);
                }
            }
            catch (Exception)
            {
                // Не критично, если не удалось удалить старый файл,
                // просто продолжаем работу.
            }
        }

        public void LogInfo(string message)
        {
            WriteToFile($"[INFO] {DateTime.Now:G}: {message}");
        }

        public void LogError(string message, Exception? ex = null)
        {
            WriteToFile($"[ERROR] {DateTime.Now:G}: {message}");
            if (ex != null)
            {
                WriteToFile(ex.ToString());
            }
        }

        private void WriteToFile(string text)
        {
            lock (_lock)
            {
                File.AppendAllText(_logFilePath, text + Environment.NewLine);
            }
        }
    }
}
