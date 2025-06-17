// Commands/RelayCommand.cs
using System;
using System.Windows.Input;

namespace GeminiChat.Wpf.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// Событие, которое WPF использует для проверки, изменилось ли состояние команды.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Метод, который определяет, может ли команда выполняться.
        /// </summary>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Метод, который выполняет логику команды.
        /// </summary>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// НОВЫЙ МЕТОД: Позволяет вручную инициировать проверку CanExecute.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            // Это заставит WPF снова вызвать CanExecute и обновить состояние кнопок.
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
