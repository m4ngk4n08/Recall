namespace Recall.Api.Services.Interfaces
{
    public interface IIngestionService
    {
        Task<Guid> IngestFromUrlAsync(string url);
    }
}
