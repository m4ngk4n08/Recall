namespace Recall.Api.DTOs.Chat
{
    public class ChatResponseDto
    {
        public string Answer { get; set; } = string.Empty;
        public Guid ConversationId { get; set; } // Tell the frontend which conversation this
        public List<SearchResultDto> Sources { get; set; }
    }
}
