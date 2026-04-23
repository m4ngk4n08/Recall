using Pgvector;
using Recall.Api.Models;
using Recall.Api.Repositories.Interfaces;
using Recall.Api.Services.Interfaces;

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
        public async Task<Guid> IngestFromUrlAsync(string url)
        {
            // 1. Extract content
            var (title, content, sourceType) = await _extractionService.ExtractFromUrlAsync(url);

            // 2. Save parent item
            var parent = new Item
            {
                Title = title,
                Content = content,
                SourceType = sourceType,
                SourceUrl = url,
                SaveAt = DateTime.UtcNow,
                Tags = new List<string>()
            };
            await _itemRepository.CreateAsync(parent);

            // 3. Chunk content
            var chunks = _extractionService.ChunkText(content);
            if (chunks.Count == 0)
                return parent.Id;

            // 4. Generate embeddings in batch
            var embeddings = await _embeddingService.GenerateEmbeddingsListAsync(chunks);

            // 5. Save each chunk as Item
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunkItem = new Item
                {
                    Title = $"{title} - Chunk {i + 1}",
                    Content = chunks[i],
                    SourceType = sourceType,
                    SourceUrl = url,
                    SaveAt = DateTime.UtcNow,
                    Tags = new List<string>(),
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
