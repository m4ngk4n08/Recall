using Recall.Api.DTOs.Ingest;

namespace Recall.Api.Services.Interfaces
{
    public interface IIngestionService
    {
        Task<Guid> IngestFromUrlAsync(string url, List<string> tags);
        Task<Guid> IngestFileAsync(Stream fileStream, string fileName, List<string> tags);
        Task<Guid> IngestThoughAsync(IngestThoughDto dto);
    }
}
