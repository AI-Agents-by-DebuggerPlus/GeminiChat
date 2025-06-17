// ViewModels/SettingsViewModel.cs
using GeminiChat.Wpf.Commands;
using GeminiChat.Wpf.Messaging; // <-- Важный using
using GeminiChat.Wpf.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace GeminiChat.Wpf.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsManager _settingsManager;
        // ИСПРАВЛЕНО: Теперь свойство имеет тип FontFamily, а не string
        private FontFamily _selectedFontFamily;
        private double _selectedFontSize;

        public ObservableCollection<FontFamily> SystemFonts { get; }
        public Action? CloseAction { get; set; }

        public FontFamily SelectedFontFamily
        {
            get => _selectedFontFamily;
            set => SetProperty(ref _selectedFontFamily, value);
        }

        public double SelectedFontSize
        {
            get => _selectedFontSize;
            set => SetProperty(ref _selectedFontSize, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public SettingsViewModel(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            SystemFonts = new ObservableCollection<FontFamily>(Fonts.SystemFontFamilies.OrderBy(f => f.Source));

            var settings = _settingsManager.LoadSettings();
            // ИСПРАВЛЕНО: Находим объект FontFamily по имени из настроек
            SelectedFontFamily = SystemFonts.FirstOrDefault(f => f.Source == settings.FontFamily) ?? SystemFonts.First();
            SelectedFontSize = settings.FontSize;

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void Save()
        {
            // Сохраняем имя шрифта, а не весь объект
            _settingsManager.SaveSettings(new AppSettings { FontFamily = this.SelectedFontFamily.Source, FontSize = this.SelectedFontSize });
            // --- ОТПРАВКА СООБЩЕНИЯ ---
            Messenger.Send(new SettingsUpdatedMessage());
            CloseAction?.Invoke();
        }

        private void Cancel()
        {
            CloseAction?.Invoke();
        }
    }
}
