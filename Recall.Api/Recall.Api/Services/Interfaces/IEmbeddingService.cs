namespace Recall.Api.Services.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string text, string? context = null);
        Task<List<float[]>> GenerateEmbeddingsListAsync(List<string> text, string? context = null);
    }
}
