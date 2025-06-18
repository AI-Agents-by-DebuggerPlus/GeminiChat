// Models/ExecutionResult.cs
namespace Executor.Models
{
    /// <summary>
    /// Представляет результат выполнения команды.
    /// </summary>
    public class ExecutionResult
    {
        /// <summary>
        /// Указывает, была ли команда выполнена успешно.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Содержит сообщение об ошибке, если команда не была выполнена успешно.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Фабричный метод для создания успешного результата.
        /// </summary>
        public static ExecutionResult Succeeded() => new() { Success = true };

        /// <summary>
        /// Фабричный метод для создания результата с ошибкой.
        /// </summary>
        public static ExecutionResult Failed(string message) => new() { Success = false, Message = message };
    }
}
