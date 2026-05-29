using AutoMapper;
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
    public class ItemsController : Controller
    {
        private readonly IITemService _iItemService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IIngestionService _ingestionService;
        private readonly IOllamaService _ollamaService;
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public ItemsController(
            IITemService iTemService,
            IEmbeddingService embeddingService,
            IIngestionService ingestionService,
            IOllamaService ollamaService,
            AppDbContext dbContext,
            IMapper mapper)
        {
            _iItemService = iTemService;
            _embeddingService = embeddingService;
            _ingestionService = ingestionService;
            _ollamaService = ollamaService;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        // ... existing methods ...


        [HttpGet("getall")]
        [ProducesResponseType(typeof(IEnumerable<ItemResponseDto>), 200)]
        public async Task<IActionResult> GetAll() =>
            Ok(await _iItemService.GetAllAsync());

        [HttpGet("topics")]
        [ProducesResponseType(typeof(IEnumerable<TopicResponseDto>), 200)]
        public async Task<IActionResult> GetTopics() =>
            Ok(await _iItemService.GetTopicsAsync());

        [HttpGet("tag/{tag}")]
        [ProducesResponseType(typeof(IEnumerable<ItemResponseDto>), 200)]
        public async Task<IActionResult> GetByTag(string tag) =>
            Ok(await _iItemService.GetByTagAsync(tag));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ItemResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _iItemService.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ItemResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] ItemCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _iItemService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ItemResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Update(Guid id, [FromBody] ItemCreateDto dto)
        {
            var updated = await _iItemService.UpdateAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ItemResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _iItemService.DeleteAsync(id);
            return deleted ? Ok() : NotFound();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Query cannot be empty.");

            // 1. Generate query embedding
            var queryVector = await _embeddingService.GenerateEmbeddingAsync(q);
            if (queryVector.Length != 384)
            {
                return BadRequest($"Query vector dimension mismatch. Expected 384, got {queryVector.Length}");
            }
            var vector = new Vector(queryVector);

            // 2. LINQ for pgvector similarity search
            // Use AsNoTracking for search-only queries
            var results = await _dbContext.Items
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
                // Convert distance to similarity for easier filtering (Similarity = 1 - Distance)
                // Lowering threshold slightly to 0.6 for better recall
                .Where(r => (1 - r.Distance) >= 0.6)
                .OrderBy(r => r.Distance)
                .Take(limit)
                .ToListAsync();

            return Ok(results);
        }

    }
}
