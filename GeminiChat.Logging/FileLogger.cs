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
            // Определяем путь к папке логов
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string logFolderPath = Path.Combine(appDataPath, "GeminiChatWpf", "Logs");

            // Убедимся, что директория существует
            Directory.CreateDirectory(logFolderPath);

            // Будем использовать один и тот же файл для всех сессий
            _logFilePath = Path.Combine(logFolderPath, "session_log.txt");

            // *** ИЗМЕНЕНИЕ ЗДЕСЬ ***
            // Убираем очистку файла и вместо этого добавляем разделитель
            // для новой сессии, чтобы сделать лог более читаемым.
            WriteToFile("\n" +
                        "====================================================\n" +
                        $"          NEW SESSION STARTED: {DateTime.Now:G}\n" +
                        "====================================================");
        }

        public void LogInfo(string message)
        {
            WriteToFile($"[INFO] {DateTime.Now:HH:mm:ss.fff}: {message}");
        }

        public void LogError(string message, Exception? ex = null)
        {
            WriteToFile($"[ERROR] {DateTime.Now:HH:mm:ss.fff}: {message}");
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
