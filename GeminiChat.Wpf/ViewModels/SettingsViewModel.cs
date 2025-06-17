// ViewModels/SettingsViewModel.cs
using GeminiChat.Wpf.Commands;
using GeminiChat.Wpf.Services;
using System;
using System.Windows.Input;

namespace GeminiChat.Wpf.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsManager _settingsManager;
        private string _fontFamily;
        private double _fontSize;

        public Action CloseAction { get; set; }

        public string FontFamily
        {
            get => _fontFamily;
            set => SetProperty(ref _fontFamily, value);
        }

        public double FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public SettingsViewModel(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;

            var settings = _settingsManager.LoadSettings();
            FontFamily = settings.FontFamily;
            FontSize = settings.FontSize;

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        private void Save()
        {
            _settingsManager.SaveSettings(new() { FontFamily = this.FontFamily, FontSize = this.FontSize });
            CloseAction?.Invoke();
        }

        private void Cancel()
        {
            CloseAction?.Invoke();
        }
    }
}
