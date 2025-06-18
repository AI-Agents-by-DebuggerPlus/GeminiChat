// Services/ICommandInterpreter.cs
using System.Threading.Tasks;

namespace GeminiChat.Wpf.Services
{
    /// <summary>
    /// Определяет сервис, который может находить и выполнять команды в тексте.
    /// </summary>
    public interface ICommandInterpreter
    {
        /// <summary>
        /// Анализирует текст, находит в нем команды и выполняет их.
        /// </summary>
        /// <param name="responseText">Текст, полученный от Gemini.</param>
        Task InterpretAndExecuteAsync(string responseText);
    }
}
