// Core/IChatService.cs
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeminiChat.Core
{
    public interface IChatService
    {
        Task<string> SendMessageAsync(string message);
        void StartNewChat();
        void LoadHistory(IEnumerable<ChatMessage> history);

        // Убедимся, что этот метод здесь есть
        void SetSystemInstruction(string instruction);
    }
}
