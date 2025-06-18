// Native/UserInput.cs
using System.Runtime.InteropServices;

namespace Executor.Native
{
    /// <summary>
    /// Вспомогательный класс для эмуляции ввода пользователя с помощью Windows API.
    /// </summary>
    internal static class UserInput
    {
        // --- Константы для событий мыши ---
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        // --- Константы для размеров экрана ---
        // ИСПРАВЛЕНО: Теперь internal, чтобы быть доступными внутри сборки Executor
        internal const int SM_CXSCREEN = 0;
        internal const int SM_CYSCREEN = 1;

        // --- Импорт системных функций ---

        [DllImport("user32.dll")]
        internal static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        internal static extern int GetSystemMetrics(int nIndex);


        public static void LeftClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public static void RightClick()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }
    }
}
