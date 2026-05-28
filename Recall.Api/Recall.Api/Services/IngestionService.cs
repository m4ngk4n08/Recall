using AngleSharp.Dom;
using Google.GenAI.Types;
using Microsoft.Extensions.AI;
using Pgvector;
using Recall.Api.Models;
using Recall.Api.Repositories.Interfaces;
using Recall.Api.Services.Interfaces;
using System.Security.AccessControl;

namespace Recall.Api.Services
{
    public class IngestionService : IIngestionService
    {
        private readonly IExtractionService _extractionService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IItemRepository _itemRepository;

        public IngestionService(IExtractionService extractionService, IEmbeddingService embeddingService, IItemRepository itemRepository)
        {
            _extractionService = extractionService;
            _embeddingService = embeddingService;
            _itemRepository = itemRepository;
        }

        public async Task<Guid> IngestFileAsync(Stream fileStream, string fileName, List<string> tags)
        {
            // 1. Extract content
            var (title, content, sourceType) = await _extractionService.ExtractPdfAsync(fileStream, fileName);

            var ingest = new IngestHelper
            {
                Title = title,
                Content = content,
                SourceType = sourceType,
                Url = null,
                Tags = tags
            };

            var parentId = await IngestHelper(ingest);

            return parentId;

        }

        public async Task<Guid> IngestFromUrlAsync(string url, List<string> tags)
        {
            // 1. Extract content
            var (title, content, sourceType) = await _extractionService.ExtractFromUrlAsync(url);

            var ingest = new IngestHelper
            {
                Title = title,
                Content = content,
                SourceType = sourceType,
                Url = url,
                Tags = tags
            };

            var parentId = await IngestHelper(ingest);

            return parentId;
        }

        private async Task<Guid> IngestHelper(IngestHelper ingest)
        {
            var parent = new Item
            {
                Title = ingest.Title,
                Content = ingest.Content,
                SourceType = ingest.SourceType,
                SourceUrl = ingest.Url,
                SavedAt = DateTime.UtcNow,
                Tags = ingest.Tags ?? new List<string>()
            };

            await _itemRepository.CreateAsync(parent);

            // 3. Chunk content
            var chunks = _extractionService.ChunkText(ingest.Content);
            if (chunks.Count == 0)
                return parent.Id;

            // 4. Generate embeddings in batch
            var embeddings = await _embeddingService.GenerateEmbeddingsListAsync(chunks);

            // 5. Save each chunk as Item
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunkItem = new Item
                {
                    Title = $"{ingest.Title} - Chunk {i + 1}",
                    Content = ingest.Content,
                    SourceType = ingest.SourceType,
                    SourceUrl = ingest.Url,
                    SavedAt = DateTime.UtcNow,
                    Tags = ingest.Tags ?? new List<string>(),
                    ParentId = parent.Id,
                    ChunkIndex = i,
                    Embedding = new Vector(embeddings[i])
                };

                await _itemRepository.CreateAsync(chunkItem);

            }

            return parent.Id;
        }
    }
}
