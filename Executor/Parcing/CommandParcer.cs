// Parsing/CommandParser.cs
using Executor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Executor.Parsing
{
    public class CommandParser
    {
        private readonly Dictionary<string, Type> _commandTypes;

        public CommandParser()
        {
            _commandTypes = new Dictionary<string, Type>();
            RegisterCommands();
        }

        public ICommand? Parse(string jsonCommand)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonCommand);
                if (!doc.RootElement.TryGetProperty("command", out var commandNameElement))
                {
                    return null;
                }

                var commandName = commandNameElement.GetString();
                if (commandName == null || !_commandTypes.TryGetValue(commandName, out var commandType))
                {
                    return null;
                }

                // --- ГЛАВНОЕ ИСПРАВЛЕНИЕ ---
                // Сначала пытаемся найти вложенный объект "parameters"
                if (doc.RootElement.TryGetProperty("parameters", out var parametersElement))
                {
                    // Если нашли, десериализуем его
                    return JsonSerializer.Deserialize(parametersElement.GetRawText(), commandType) as ICommand;
                }
                else
                {
                    // Если вложенного объекта нет, считаем, что структура "плоская"
                    // и десериализуем весь корневой JSON.
                    return JsonSerializer.Deserialize(jsonCommand, commandType) as ICommand;
                }
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private void RegisterCommands()
        {
            var commandInterface = typeof(ICommand);
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(p => commandInterface.IsAssignableFrom(p) && !p.IsInterface);

            foreach (var type in types)
            {
                var commandNameProperty = type.GetProperty("CommandName", BindingFlags.Public | BindingFlags.Static);
                if (commandNameProperty != null)
                {
                    var commandName = commandNameProperty.GetValue(null) as string;
                    if (commandName != null)
                    {
                        _commandTypes[commandName] = type;
                    }
                }
            }
        }
    }
}
