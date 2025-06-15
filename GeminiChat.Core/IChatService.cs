using System.Threading.Tasks;

namespace GeminiChat.Core
{
    public interface IChatService
    {
        Task<string> SendMessageAsync(string message);
    }
}