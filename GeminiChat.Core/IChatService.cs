using GeminiChat.Core;
using System.Collections.Generic; // <-- Добавить
using System.Threading.Tasks;

namespace GeminiChat.Core
{
    public interface IChatService
    {
        // Старый метод остается
        Task<string> SendMessageAsync(string message);

        // Новый метод для загрузки истории в сервис
        void PrimeHistory(IEnumerable<ChatMessage> history);
    }
}
