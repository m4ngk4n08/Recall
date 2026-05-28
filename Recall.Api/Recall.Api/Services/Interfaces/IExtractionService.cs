namespace Recall.Api.Services.Interfaces
{
    public interface IExtractionService
    {
        Task<(string Title, string Content, string SourceType)> ExtractFromUrlAsync(string url);
        List<string> ChunkText(string text, int maxToken = 200);
        Task<(string Title, string Content, string SourceType)> ExtractPdfAsync(Stream stream, string fileName);
    }
}
