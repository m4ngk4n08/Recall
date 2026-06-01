namespace Recall.Api.Models
{
    public class Conversation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
