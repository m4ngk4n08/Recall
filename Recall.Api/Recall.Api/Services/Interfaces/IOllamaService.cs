namespace Recall.Api.Services.Interfaces
{
    public interface IOllamaService
    {
        Task<string> GenerateAnswerAsync(string prompt, string context, string model = "llama3");
    }
}
