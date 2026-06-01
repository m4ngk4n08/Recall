namespace Recall.Api.DTOs.Chat
{
    public class ChatRequestDto
    {
        public string Query { get; set; } = string.Empty;
        public string Model { get; set; } = "llama3";
        public Guid? ConversationId { get; set; }
    }
}
