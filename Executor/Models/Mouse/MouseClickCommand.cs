// Models/Mouse/MouseClickCommand.cs
using System.Text.Json.Serialization;

namespace Executor.Models.Mouse
{
    /// <summary>
    /// Команда для эмуляции клика мыши.
    /// </summary>
    public class MouseClickCommand : ICommand
    {
        /// <summary>
        /// Уникальное имя команды.
        /// </summary>
        [JsonIgnore]
        public static string CommandName => "mouse.click";

        /// <summary>
        /// Координата X для клика. Может быть числом или строкой (например, "center").
        /// </summary>
        [JsonPropertyName("x")]
        public object X { get; set; } = "center";

        /// <summary>
        /// Координата Y для клика. Может быть числом или строкой (например, "center").
        /// </summary>
        [JsonPropertyName("y")]
        public object Y { get; set; } = "center";

        /// <summary>
        /// Кнопка мыши ("left" или "right").
        /// </summary>
        [JsonPropertyName("button")]
        public string Button { get; set; } = "left";

        /// <summary>
        /// Указывает, должен ли клик быть двойным.
        /// </summary>
        [JsonPropertyName("doubleClick")]
        public bool DoubleClick { get; set; } = false;
    }
}
