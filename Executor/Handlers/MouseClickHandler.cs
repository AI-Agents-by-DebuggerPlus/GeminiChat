// Handlers/MouseClickHandler.cs
using Executor.Models;
using Executor.Models.Mouse;
using Executor.Native;
using System;
using System.Text.Json; // <-- Важный using для JsonElement
using System.Threading.Tasks;

namespace Executor.Handlers
{
    public class MouseClickHandler : ICommandHandler<MouseClickCommand>
    {
        private record struct Point(double X, double Y);

        public Task<ExecutionResult> HandleAsync(MouseClickCommand command)
        {
            try
            {
                Point targetPoint = GetCoordinates(command.X, command.Y);
                UserInput.SetCursorPos((int)targetPoint.X, (int)targetPoint.Y);

                if (command.Button.ToLower() == "left")
                {
                    UserInput.LeftClick();
                    if (command.DoubleClick) UserInput.LeftClick();
                }
                else if (command.Button.ToLower() == "right")
                {
                    UserInput.RightClick();
                }
                else
                {
                    return Task.FromResult(ExecutionResult.Failed($"Unknown mouse button: {command.Button}"));
                }
                return Task.FromResult(ExecutionResult.Succeeded());
            }
            catch (Exception ex)
            {
                return Task.FromResult(ExecutionResult.Failed($"Failed to execute mouse click: {ex.Message}"));
            }
        }

        /// <summary>
        /// Вспомогательный метод для преобразования строковых или числовых координат в точки.
        /// </summary>
        private Point GetCoordinates(object xObj, object yObj)
        {
            double x = ParseCoordinate(xObj, UserInput.GetSystemMetrics(UserInput.SM_CXSCREEN));
            double y = ParseCoordinate(yObj, UserInput.GetSystemMetrics(UserInput.SM_CYSCREEN));

            return new Point(x, y);
        }

        /// <summary>
        /// Универсальный парсер для одной координаты.
        /// </summary>
        private double ParseCoordinate(object coordObj, double totalSize)
        {
            // Наш объект всегда приходит как JsonElement
            if (coordObj is not JsonElement element)
            {
                // Если это что-то другое, пытаемся преобразовать напрямую (запасной вариант)
                return Convert.ToDouble(coordObj);
            }

            // --- ГЛАВНОЕ ИСПРАВЛЕНИЕ: Проверяем тип данных внутри JsonElement ---

            // Если внутри число, просто возвращаем его
            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetDouble();
            }

            // Если внутри строка, анализируем ее
            if (element.ValueKind == JsonValueKind.String)
            {
                var strValue = element.GetString()?.ToLower().Trim();
                if (strValue == "center")
                {
                    return totalSize / 2;
                }
                if (strValue != null && strValue.EndsWith('%'))
                {
                    if (double.TryParse(strValue.TrimEnd('%'), out double percentage))
                    {
                        return totalSize * (percentage / 100.0);
                    }
                }
            }

            // Если мы дошли сюда, значит, формат координаты неизвестен
            throw new ArgumentException($"Unsupported coordinate format: {coordObj}");
        }
    }
}
