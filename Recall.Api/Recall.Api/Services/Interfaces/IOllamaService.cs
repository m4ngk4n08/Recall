using Microsoft.Extensions.AI;
using Recall.Api.DTOs.Chat;

namespace Recall.Api.Services.Interfaces
{
    public interface IOllamaService
    {
        Task<string> GenerateAnswerAsync(string prompt, string context, string model = "llama3");

        Task<string> GenerateChatResponseAsync(string query, string context, List<ChatMessageDto> conversationHistory, string model = "llama3");
    }
}
