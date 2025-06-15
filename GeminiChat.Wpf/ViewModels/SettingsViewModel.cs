using GeminiChat.Wpf.Commands;
using GeminiChat.Wpf.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace GeminiChat.Wpf.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsManager _settingsManager;
        private FontFamily _selectedFontFamily;
        private double _selectedFontSize;

        // Событие, которое попросит окно закрыться
        public Action? CloseAction { get; set; }

        public ICollection<FontFamily> SystemFonts { get; } = Fonts.SystemFontFamilies
            .OrderBy(f => f.Source)
            .ToList();

        public FontFamily SelectedFontFamily
        {
            get => _selectedFontFamily;
            set { _selectedFontFamily = value; OnPropertyChanged(); }
        }

        public double SelectedFontSize
        {
            get => _selectedFontSize;
            set { _selectedFontSize = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }

        public SettingsViewModel(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;

            SelectedFontFamily = _settingsManager.ChatFontFamily;
            SelectedFontSize = _settingsManager.ChatFontSize;

            SaveCommand = new RelayCommand(_ => OnSave());
        }

        private void OnSave()
        {
            // 1. Обновляем глобальный сервис настроек
            _settingsManager.ChatFontFamily = SelectedFontFamily;
            _settingsManager.ChatFontSize = SelectedFontSize;

            // 2. Сохраняем настройки в файл
            _settingsManager.SaveSettings();

            // 3. Вызываем событие, чтобы окно закрылось
            CloseAction?.Invoke();
        }
    }
}
