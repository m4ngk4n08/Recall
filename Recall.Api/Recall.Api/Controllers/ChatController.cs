using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Recall.Api.Data;
using Recall.Api.DTOs;
using Recall.Api.DTOs.Chat;
using Recall.Api.Models;
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

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var list = await _dbContext.Conversations
                .OrderByDescending(c => c.Messages.Max(m => m.Timestamp))
                .Select(j => new { 
                    j.Id, 
                    j.Title,
                    LastMessageAt = j.Messages.Max(j => j.Timestamp),
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return BadRequest("Query cannot be empty.");

            // 1. Get or Create Conversation
            Conversations? conversation;
            if (request.ConversationId.HasValue)
            {
                conversation = await _dbContext.Conversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == request.ConversationId.Value);

                if(conversation == null)
                    return NotFound("Conversation not found.");
            }
            else
            {
                conversation = new Conversations { Title = request.Query.Length > 50 ? request.Query[..50] : request.Query };
                _dbContext.Conversations.Add(conversation);
            }

            // 2. Save User Message
            var userMessage = new ChatMessage
            {
                ConversationId = conversation.Id,
                Role = "user",
                Content = request.Query
            };

            _dbContext.ChatMessages.Add(userMessage);
            await _dbContext.SaveChangesAsync();

            // 3. Get history for Ollama(last 10 messages)
            var history = await _dbContext.ChatMessages
                .Where(m => m.ConversationId == conversation.Id && m.Id != userMessage.Id)
                .OrderByDescending(m => m.Timestamp)
                .Take(10)
                .OrderBy(m => m.Timestamp)
                .Select(m => new ChatMessageDto { Role = m.Role, Content = m.Content })
                .ToListAsync();

            // 4. Retrieve Document Context(RAG)
            var queryVector = await _embeddingService.GenerateEmbeddingAsync(request.Query);
            var vector = new Pgvector.Vector(queryVector);

            var sources = await _dbContext.Items
                .AsNoTracking()
                .Where(i => i.Embedding != null)
                .Select(i => new SearchResultDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    Content = i.Content,
                    SourceUrl = i.SourceUrl,
                    Distance = vector.CosineDistance(i.Embedding)
                })
                .Where(r => (1 - r.Distance) >= 0.5)
                .OrderBy(r => r.Distance)
                .Take(5)
                .ToListAsync();

            var context = string.Join("\n\n", sources.Select(s => $"Title: {s.Title}\nContent: {s.Content}"));

            // 5. Generate AI Response
            var answer = await _ollamaService.GenerateChatResponseAsync(request.Query, context, history, request.Model);

            // 6. Save AI Message
            var assistantMessage = new ChatMessage
            {
                ConversationId = conversation.Id,
                Role = "assistant",
                Content = answer
            };
            
            _dbContext.ChatMessages.Add(assistantMessage);
            await _dbContext.SaveChangesAsync();

            return Ok(new ChatResponseDto
            {
                Answer = answer,
                ConversationId = conversation.Id,
                Sources = sources
            });
        }

        [HttpGet("history/{conversationId}")]
        public async Task<IActionResult> GetChatHistory(Guid conversationId)
        {
            // Fetch all mesages for this conversation, ordered by time
            var messages = await _dbContext.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.Timestamp)
                .Select(m => new ChatMessageDto
                {
                    Role = m.Role,
                    Content = m.Content,
                    TimeStamp = m.Timestamp
                })
                .ToListAsync();

            if(messages == null || messages.Count == 0)
            {
                return NotFound("No history found for this conversation.");
            }

            return Ok(messages);
        }
    }
}
