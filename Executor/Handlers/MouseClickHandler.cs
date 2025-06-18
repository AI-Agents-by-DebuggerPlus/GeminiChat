// Handlers/MouseClickHandler.cs
using Executor.Models;
using Executor.Models.Mouse;
using Executor.Native;
using System;
using System.Text.Json; // Важный using для JsonElement
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
            double x, y;
            double screenWidth = UserInput.GetSystemMetrics(UserInput.SM_CXSCREEN);
            double screenHeight = UserInput.GetSystemMetrics(UserInput.SM_CYSCREEN);

            // --- ГЛАВНОЕ ИСПРАВЛЕНИЕ: Корректно обрабатываем все типы данных ---
            x = ParseCoordinate(xObj, screenWidth, "center");
            y = ParseCoordinate(yObj, screenHeight, "center");

            return new Point(x, y);
        }

        /// <summary>
        /// Универсальный парсер для одной координаты.
        /// </summary>
        private double ParseCoordinate(object coordObj, double totalSize, string centerKeyword)
        {
            // Случай 1: Это уже число (например, из JSON `{"x": 100}`)
            if (coordObj is JsonElement elem && elem.ValueKind == JsonValueKind.Number)
            {
                return elem.GetDouble();
            }

            // Случай 2: Это строка (например, из JSON `{"x": "center"}` или `{"x": "50%"}`)
            if (coordObj is JsonElement strElem && strElem.ValueKind == JsonValueKind.String)
            {
                var strValue = strElem.GetString()?.ToLower().Trim();
                if (strValue == centerKeyword)
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

            // Случай 3: Если ничего не подошло, пытаемся преобразовать напрямую
            return Convert.ToDouble(coordObj);
        }
    }
}
