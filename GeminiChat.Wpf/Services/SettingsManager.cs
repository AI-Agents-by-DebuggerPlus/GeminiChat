// Services/SettingsManager.cs
using System;
using System.IO;
using System.Text.Json;

namespace GeminiChat.Wpf.Services
{
    /// <summary>
    /// Класс для хранения данных настроек.
    /// </summary>
    public class AppSettings
    {
        public string FontFamily { get; set; } = "Segoe UI";
        public double FontSize { get; set; } = 15;
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
            var appDir = Path.Combine(appDataPath, "GeminiChat");
            Directory.CreateDirectory(appDir);
            _filePath = Path.Combine(appDir, "settings.json");
        }

        /// <summary>
        /// Загружает настройки из файла. Метод теперь публичный и возвращает AppSettings.
        /// </summary>
        public AppSettings LoadSettings()
        {
            if (!File.Exists(_filePath))
            {
                return new AppSettings();
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

        /// <summary>
        /// Сохраняет настройки в файл. Метод теперь публичный.
        /// </summary>
        public void SaveSettings(AppSettings settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch
            {
                // В реальном приложении здесь должно быть логирование
            }
        }
    }
}
