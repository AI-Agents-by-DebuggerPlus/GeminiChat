// Services/SettingsManager.cs
using System;
using System.IO;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace GeminiChat.Wpf.Services
{
    /// <summary>
    /// Класс для хранения данных настроек.
    /// </summary>
    public class AppSettings
    {
        public string FontFamily { get; set; } = "Segoe UI";
        public double FontSize { get; set; } = 15;
        // --- НОВОЕ СВОЙСТВО ---
        public string? ApiKey { get; set; }
    }

    /// <summary>
    /// Сервис для управления настройками приложения.
    /// </summary>
    public class SettingsManager
    {
        private readonly string _filePath;

        public SettingsManager()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appDir = Path.Combine(appDataPath, "GeminiChatWpf"); // Используем уникальное имя
            Directory.CreateDirectory(appDir);
            _filePath = Path.Combine(appDir, "settings.json");
        }

        public AppSettings LoadSettings()
        {
            if (!File.Exists(_filePath))
            {
                // Если файла нет, создаем и возвращаем настройки по умолчанию
                var defaultSettings = new AppSettings();
                SaveSettings(defaultSettings);
                return defaultSettings;
            }

            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                // Добавляем опции для корректной сериализации кириллицы
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                };
                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_filePath, json);
            }
            catch
            {
                // В реальном приложении здесь должно быть логирование
            }
        }
    }
}
