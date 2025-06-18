// Services/CommandInterpreter.cs
using Executor; // <-- Важный using для доступа к нашей новой библиотеке
using GeminiChat.Core;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GeminiChat.Wpf.Services
{
    public class CommandInterpreter : ICommandInterpreter
    {
        private readonly CommandExecutor _commandExecutor;
        private readonly ILogger _logger;
        private const string CommandBlockMarker = "%%COMMAND_JSON%%";

        public CommandInterpreter(CommandExecutor commandExecutor, ILogger logger)
        {
            _commandExecutor = commandExecutor;
            _logger = logger;
        }

        public async Task InterpretAndExecuteAsync(string responseText)
        {
            // Ищем наш специальный маркер в тексте
            if (!responseText.Contains(CommandBlockMarker))
            {
                return; // Команд нет, выходим
            }

            // Регулярное выражение для извлечения JSON-блока
            var regex = new Regex($@"{CommandBlockMarker}\s*```json\s*([\s\S]*?)\s*```");
            var match = regex.Match(responseText);

            if (match.Success)
            {
                var jsonCommand = match.Groups[1].Value.Trim();
                _logger.LogInfo($"[Interpreter] Found command JSON: {jsonCommand}");

                // Передаем команду нашему исполнителю
                var result = await _commandExecutor.ExecuteAsync(jsonCommand);

                if (!result.Success)
                {
                    _logger.LogError($"[Interpreter] Command execution failed: {result.Message}");
                    // Здесь можно добавить логику для оповещения пользователя об ошибке
                }
                else
                {
                    _logger.LogInfo("[Interpreter] Command executed successfully.");
                }
            }
        }
    }
}
