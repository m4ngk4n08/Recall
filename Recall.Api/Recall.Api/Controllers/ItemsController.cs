using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Recall.Api.Data;
using Recall.Api.DTOs;
using Recall.Api.Services.Interfaces;

namespace Recall.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : Controller
    {
        private readonly IITemService _iItemService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IIngestionService _ingestionService;
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;

        public ItemsController(
            IITemService iTemService,
            IEmbeddingService embeddingService,
            IIngestionService ingestionService,
            AppDbContext dbContext,
            IMapper mapper)
        {
            _iItemService = iTemService;
            _embeddingService = embeddingService;
            _ingestionService = ingestionService;
            _dbContext = dbContext;
            _mapper = mapper;
        }
        //public async Task<IActionResult> Index()
        //{
        //    return View();
        //}

        [HttpGet("getall")]
        [ProducesResponseType(typeof(IEnumerable<ItemResponseDto>), 200)]
        public async Task<IActionResult> GetAll() =>
            Ok(await _iItemService.GetAllAsync());

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

        [HttpPost("ingest")]
        [ProducesResponseType(typeof(ItemResponseDto), 200)]
        public async Task<IActionResult> IngestUrl([FromBody] IngestUrlDto dto)
        {
            if(string.IsNullOrEmpty(dto.Url)) 
                return BadRequest("URL cannot be empty.");

            // Run ingestion in background (fire and forget for simplicity, but we'll wait)
            var parentId = await _ingestionService.IngestFromUrlAsync(dto.Url);
            return Accepted(new {jobId = parentId, message = "Ingestion started. Check back later for results." });
        }

        [HttpGet("debug")]
        public async Task<IActionResult> Debug()
        {
            var total = await _dbContext.Items.CountAsync();
            var withParent = await _dbContext.Items.CountAsync(i => i.ParentId != null);
            var withEmbedding = await _dbContext.Items.CountAsync(i => i.Embedding != null);
            return Ok(new { total, withParent, withEmbedding });
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
            // We search in items that have embeddings
            var items = await _dbContext.Items
                .Where(i => i.Embedding != null)
                .OrderBy(i => i.Embedding!.CosineDistance(vector))
                .Take(limit)
                .ToListAsync();

            if (items.Count == 0)
            {
                return Ok(new List<SearchResultDto>());
            }

            // 3. Map to DTO in memory to avoid translation issues
            var results = items.Select(i => new SearchResultDto
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
                Distance = i.Embedding != null ? (double)i.Embedding.CosineDistance(vector) : 0
            }).ToList();

            return Ok(results);
        }
    }
}
