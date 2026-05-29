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
    public class IngestController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IIngestionService _ingestionService;
        private readonly IEmbeddingService _embeddingService;

        public IngestController(
            AppDbContext dbContext,
            IIngestionService ingestionService,
            IEmbeddingService embeddingService)
        {
            _dbContext = dbContext;
            _ingestionService = ingestionService;
            _embeddingService = embeddingService;
        }

        [HttpPost("Url")]
        [ProducesResponseType(typeof(ItemResponseDto), 200)]
        public async Task<IActionResult> Url([FromBody] IngestUrlDto dto)
        {
            if (string.IsNullOrEmpty(dto.Url))
                return BadRequest("URL cannot be empty.");

            // Run ingestion in background (fire and forget for simplicity, but we'll wait)
            var parentId = await _ingestionService.IngestFromUrlAsync(dto.Url, dto.Tags);
            return Accepted(new { jobId = parentId, message = "Ingestion started. Check back later for results." });
        }

        [HttpPost("Pdf")]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Pdf(IFormFile file, [FromForm] string tags = "")
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
                return BadRequest("Only PDF files are supported.");

            // Parse tags from comma-separated string if provided
            var tagList = tags?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(j => j.Trim())
                .ToList() ?? new List<string>();

            using var stream = file.OpenReadStream();

            //ingest the file
            var parentId = await _ingestionService.IngestFileAsync(stream, file.FileName, tagList);

            return Accepted(new { jobId = parentId, message = "PDF Ingestion completed successfully." });
        }
    }
}
