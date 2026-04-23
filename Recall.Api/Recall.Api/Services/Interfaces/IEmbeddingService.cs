namespace Recall.Api.Services.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string text);
        Task<List<float[]>> GenerateEmbeddingsListAsync(List<string> text);
    }
}
