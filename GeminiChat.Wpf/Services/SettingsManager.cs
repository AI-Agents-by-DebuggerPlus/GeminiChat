using GeminiChat.Wpf.ViewModels;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows.Media;

namespace GeminiChat.Wpf.Services
{
    public class SettingsManager : ViewModelBase
    {
        // Внутренний класс для удобной сериализации
        private class AppSettingsModel
        {
            public string? FontName { get; set; }
            public double FontSize { get; set; }
        }

        private readonly string _settingsFilePath;

        private FontFamily _chatFontFamily = new FontFamily("Segoe UI");
        private double _chatFontSize = 15;

        public FontFamily ChatFontFamily
        {
            get => _chatFontFamily;
            set { _chatFontFamily = value; OnPropertyChanged(); }
        }

        public double ChatFontSize
        {
            get => _chatFontSize;
            set { _chatFontSize = value; OnPropertyChanged(); }
        }

        public SettingsManager()
        {
            // Определяем путь к файлу настроек в папке данных пользователя
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolderPath = Path.Combine(appDataPath, "GeminiChatWpf");
            _settingsFilePath = Path.Combine(appFolderPath, "settings.json");

            // Загружаем настройки при старте
            LoadSettings();
        }

        public void SaveSettings()
        {
            var settingsModel = new AppSettingsModel
            {
                FontName = this.ChatFontFamily.Source,
                FontSize = this.ChatFontSize
            };

            // *** ИСПРАВЛЕНИЕ ЗДЕСЬ ***
            // Указываем полное имя, чтобы избежать двусмысленности
            string json = JsonConvert.SerializeObject(settingsModel, Newtonsoft.Json.Formatting.Indented);

            // Убедимся, что директория существует
            Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath));

            File.WriteAllText(_settingsFilePath, json);
        }

        private void LoadSettings()
        {
            if (!File.Exists(_settingsFilePath)) return;

            try
            {
                string json = File.ReadAllText(_settingsFilePath);
                var settingsModel = JsonConvert.DeserializeObject<AppSettingsModel>(json);

                if (settingsModel != null)
                {
                    if (!string.IsNullOrEmpty(settingsModel.FontName))
                    {
                        this.ChatFontFamily = new FontFamily(settingsModel.FontName);
                    }
                    // Добавим проверку, чтобы размер шрифта был в разумных пределах
                    if (settingsModel.FontSize >= 10 && settingsModel.FontSize <= 32)
                    {
                        this.ChatFontSize = settingsModel.FontSize;
                    }
                }
            }
            catch (Exception)
            {
                // Если файл поврежден, просто используем настройки по умолчанию.
                // В реальном приложении здесь стоит добавить логирование ошибки.
            }
        }
    }
}
