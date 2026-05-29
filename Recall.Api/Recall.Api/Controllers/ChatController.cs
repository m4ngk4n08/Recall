using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Recall.Api.Data;
using Recall.Api.DTOs;
using Recall.Api.Services.Interfaces;
using System.Text;

namespace Recall.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IOllamaService _ollamaService;
        private readonly AppDbContext _dbContext;

        public ChatController(
            IEmbeddingService embeddingService,
            IOllamaService ollamaService,
            AppDbContext dbContext)
        {
            _embeddingService = embeddingService;
            _ollamaService = ollamaService;
            _dbContext = dbContext;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return BadRequest("Query cannot be empty.");

            // 1. Retrieve relevant context (Reuse search logic)
            var queryVector = await _embeddingService.GenerateEmbeddingAsync(request.Query);
            var vector = new Vector(queryVector);

            var sources = await _dbContext.Items
                .AsNoTracking()
                .Where(i => i.Embedding != null)
                .Select(i => new SearchResultDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    Content = i.Content,
                    SourceType = i.SourceType,
                    SourceUrl = i.SourceUrl,
                    SavedAt = i.SavedAt,
                    Tags = i.Tags,
                    ParentId = i.ParentId,
                    ChunkIndex = i.ChunkIndex,
                    Distance = i.Embedding.CosineDistance(vector)
                })
                .Where(r => (1 - r.Distance) >= 0.5) // Slightly more lenient for chat context
                .OrderBy(r => r.Distance)
                .Take(5) // Top 5 chunks for context
                .ToListAsync();

            if (!sources.Any())
            {
                return Ok(new ChatResponseDto
                {
                    Answer = "I couldn't find any relevant information in your documents to answer this question.",
                    Sources = new List<SearchResultDto>()
                });
            }

            // 2. Format context for the LLM
            var contextBuilder = new StringBuilder();
            foreach (var source in sources)
            {
                contextBuilder.AppendLine($"--- Document: {source.Title} ---");
                contextBuilder.AppendLine(source.Content);
                contextBuilder.AppendLine();
            }

            // 3. Generate answer using Ollama
            var answer = await _ollamaService.GenerateAnswerAsync(request.Query, contextBuilder.ToString(), request.Model);

            return Ok(new ChatResponseDto
            {
                Answer = answer,
                Sources = sources
            });
        }
    }
}
