// CommandExecutor.cs
using Executor.Handlers;
using Executor.Models;
using Executor.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Executor
{
    /// <summary>
    /// Главный класс, который принимает JSON-команды и выполняет их.
    /// </summary>
    public class CommandExecutor
    {
        private readonly CommandParser _parser;
        // Словарь для хранения обработчиков.
        // Ключ - тип команды (напр., typeof(MouseClickCommand)), значение - объект-обработчик.
        private readonly Dictionary<Type, object> _handlers;

        public CommandExecutor()
        {
            _parser = new CommandParser();
            _handlers = new Dictionary<Type, object>();
            // Автоматически находим и регистрируем все обработчики при создании.
            RegisterHandlers();
        }

        /// <summary>
        /// Главный публичный метод для выполнения команды.
        /// </summary>
        /// <param name="jsonCommand">Строка в формате JSON, описывающая команду.</param>
        /// <returns>Результат выполнения команды.</returns>
        public async Task<ExecutionResult> ExecuteAsync(string jsonCommand)
        {
            // Шаг 1: Парсим JSON в объект команды.
            var command = _parser.Parse(jsonCommand);

            if (command == null)
            {
                return ExecutionResult.Failed("Unknown or malformed command.");
            }

            // Шаг 2: Находим нужный обработчик для этого типа команды.
            if (!_handlers.TryGetValue(command.GetType(), out var handler))
            {
                return ExecutionResult.Failed($"No handler found for command type: {command.GetType().Name}");
            }

            // Шаг 3: Динамически вызываем метод HandleAsync у найденного обработчика.
            // Этот сложный код нужен, потому что мы работаем с универсальными типами (generics).
            var method = handler.GetType().GetMethod("HandleAsync");
            if (method == null)
            {
                return ExecutionResult.Failed($"Handler for {command.GetType().Name} does not have a HandleAsync method.");
            }

            // Вызываем метод, передавая ему объект команды, и ждем результат.
            var task = (Task<ExecutionResult>?)method.Invoke(handler, new object[] { command });
            if (task == null)
            {
                return ExecutionResult.Failed("Failed to invoke HandleAsync method.");
            }

            return await task;
        }

        /// <summary>
        /// Сканирует сборку, находит все классы, реализующие ICommandHandler<T>,
        /// и создает их экземпляры.
        /// </summary>
        private void RegisterHandlers()
        {
            // Ищем все типы, которые реализуют наш универсальный интерфейс ICommandHandler<T>
            var handlerInterface = typeof(ICommandHandler<>);
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(p => p.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface));

            foreach (var handlerType in types)
            {
                // Находим, какой именно интерфейс он реализует (например, ICommandHandler<MouseClickCommand>)
                var implementedInterface = handlerType.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface);

                // Извлекаем тип команды (например, MouseClickCommand)
                var commandType = implementedInterface.GetGenericArguments()[0];

                // Создаем экземпляр обработчика (например, new MouseClickHandler())
                var handlerInstance = Activator.CreateInstance(handlerType);
                if (handlerInstance != null)
                {
                    // Добавляем пару "тип_команды" -> "обработчик" в словарь.
                    _handlers[commandType] = handlerInstance;
                }
            }
        }
    }
}
