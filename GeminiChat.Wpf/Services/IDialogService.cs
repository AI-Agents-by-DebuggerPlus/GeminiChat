// Services/IDialogService.cs
namespace GeminiChat.Wpf.Services
{
    public interface IDialogService
    {
        void ShowSettingsDialog();

        /// <summary>
        /// Проверяет наличие ключа и при необходимости показывает окно настроек.
        /// </summary>
        /// <returns>True, если ключ есть и можно продолжать; False, если приложение нужно закрыть.</returns>
        bool EnsureApiKeyIsSet();
    }
}
