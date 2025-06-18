// ViewModels/SettingsViewModel.cs
using GeminiChat.Core;
using GeminiChat.Wpf.Commands;
using GeminiChat.Wpf.Messaging;
using GeminiChat.Wpf.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GeminiChat.Wpf.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly SettingsManager? _settingsManager;
        private readonly ILogger? _logger;
        private FontFamily _selectedFontFamily;
        private double _selectedFontSize;
        private string _actualApiKey = "";
        private bool _isApiKeyFocused = false;

        public string ApiKeyDisplay
        {
            get => _isApiKeyFocused || string.IsNullOrEmpty(_actualApiKey) ? _actualApiKey : MaskApiKey(_actualApiKey);
            set
            {
                if (SetProperty(ref _actualApiKey, value, nameof(ApiKeyDisplay)))
                {
                    OnPropertyChanged(nameof(IsApiKeyValid));
                }
            }
        }

        public bool IsApiKeyValid => !string.IsNullOrEmpty(_actualApiKey);
        public ObservableCollection<FontFamily> SystemFonts { get; }
        public Action? CloseAction { get; set; }
        public FontFamily SelectedFontFamily { get => _selectedFontFamily; set => SetProperty(ref _selectedFontFamily, value); }
        public double SelectedFontSize { get => _selectedFontSize; set => SetProperty(ref _selectedFontSize, value); }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ApiKeyGotFocusCommand { get; }
        public ICommand ApiKeyLostFocusCommand { get; }

        public SettingsViewModel(SettingsManager settingsManager, ILogger logger)
        {
            _settingsManager = settingsManager;
            _logger = logger;
            SystemFonts = new ObservableCollection<FontFamily>(Fonts.SystemFontFamilies.OrderBy(f => f.Source));

            var settings = _settingsManager.LoadSettings();
            SelectedFontFamily = SystemFonts.FirstOrDefault(f => f.Source == settings.FontFamily) ?? SystemFonts.First();
            SelectedFontSize = settings.FontSize;
            _actualApiKey = settings.ApiKey ?? "";

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => CloseAction?.Invoke());
            ApiKeyGotFocusCommand = new RelayCommand(_ => OnApiKeyFocus(true));
            ApiKeyLostFocusCommand = new RelayCommand(_ => OnApiKeyFocus(false));
        }

        private void OnApiKeyFocus(bool isFocused)
        {
            _isApiKeyFocused = isFocused;
            OnPropertyChanged(nameof(ApiKeyDisplay));
        }

        /// <summary>
        /// ИСПРАВЛЕННЫЙ МЕТОД: Маскирует API-ключ, заменяя среднюю часть звездочками.
        /// </summary>
        private string MaskApiKey(string apiKey)
        {
            // Если ключ слишком короткий для маскировки, возвращаем его как есть
            // или показываем только звездочки, чтобы не выдать его длину.
            if (string.IsNullOrEmpty(apiKey) || apiKey.Length <= 7)
            {
                return new string('*', apiKey?.Length ?? 0);
            }

            var start = apiKey.Substring(0, 3);
            var end = apiKey.Substring(apiKey.Length - 4);

            // Вычисляем, сколько символов нужно скрыть
            var middlePartLength = apiKey.Length - start.Length - end.Length;
            var asterisks = new string('*', middlePartLength);

            return $"{start}{asterisks}{end}";
        }

        private void Save()
        {
            _logger?.LogInfo("[Settings] Save method initiated.");

            if (string.IsNullOrWhiteSpace(_actualApiKey))
            {
                _logger?.LogWarning("[Settings] Save failed: API Key is empty.");
                MessageBox.Show("Поле API ключа не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_settingsManager == null) return;
            var newSettings = new AppSettings
            {
                FontFamily = this.SelectedFontFamily.Source,
                FontSize = this.SelectedFontSize,
                ApiKey = this._actualApiKey
            };

            _logger?.LogInfo($"[Settings] Saving new settings...");
            _settingsManager.SaveSettings(newSettings);

            _logger?.LogInfo("[Settings] Settings saved successfully. Sending update message.");
            Messenger.Send(new SettingsUpdatedMessage());

            _logger?.LogInfo("[Settings] Invoking CloseAction to close the window.");
            CloseAction?.Invoke();
        }
    }
}
