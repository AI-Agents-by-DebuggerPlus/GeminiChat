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
            // *** ИЗМЕНЕНИЕ ЗДЕСЬ ***
            // Указываем новый, жестко заданный путь к папке логов.
            string logFolderPath = @"d:\Programming\Debug\Logs\GeminiChat\";

            // Убедимся, что директория существует. Если нет - создаем ее.
            Directory.CreateDirectory(logFolderPath);

            // Имя файла остается прежним.
            _logFilePath = Path.Combine(logFolderPath, "session_log.txt");

            // Добавляем разделитель для новой сессии.
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
                // Используем File.AppendAllText, чтобы дописывать в конец файла.
                File.AppendAllText(_logFilePath, text + Environment.NewLine);
            }
        }
    }
}
