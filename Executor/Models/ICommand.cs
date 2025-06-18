// Models/ICommand.cs
using System.Text.Json.Serialization;

namespace Executor.Models
{
    /// <summary>
    /// Базовый интерфейс для всех исполняемых команд.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Уникальное имя команды в формате "пространство.действие", например, "mouse.click".
        /// Это свойство не будет сериализоваться в JSON, так как оно используется для идентификации.
        /// </summary>
        [JsonIgnore]
        static abstract string CommandName { get; }
    }
}
