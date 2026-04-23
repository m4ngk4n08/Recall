namespace Recall.Api.Services.Interfaces
{
    public interface IExtractionService
    {
        Task<(string Title, string Content, string SourceType)> ExtractFromUrlAsync(string url);
        List<string> ChunkText(string text, int chunkSize = 500, int overlap = 50);

    }
}
