// Handlers/ICommandHandler.cs
using Executor.Models;
using System.Threading.Tasks;

namespace Executor.Handlers
{
    /// <summary>
    /// Универсальный интерфейс для обработчика команды.
    /// </summary>
    /// <typeparam name="TCommand">Тип команды, которую этот обработчик выполняет.</typeparam>
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        /// <summary>
        /// Выполняет логику команды.
        /// </summary>
        /// <param name="command">Объект команды с параметрами.</param>
        /// <returns>Результат выполнения.</returns>
        Task<ExecutionResult> HandleAsync(TCommand command);
    }
}
