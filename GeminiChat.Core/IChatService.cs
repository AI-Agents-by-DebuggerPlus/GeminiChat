using GeminiChat.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeminiChat.Core
{
    public interface IChatService
    {
        Task<string> SendMessageAsync(string message);

        // Метод теперь асинхронный, так как он будет делать вызов API
        Task PrimeContextAsync(IEnumerable<ChatMessage> history);
    }
}
