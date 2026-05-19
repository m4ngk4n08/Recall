namespace Recall.Api.DTOs
{
    public class ChatRequestDto
    {
        public string Query { get; set; } = string.Empty;
        public string Model { get; set; } = "llama3"; // Default model
    }

    public class ChatResponseDto
    {
        public string Answer { get; set; } = string.Empty;
        public List<SearchResultDto> Sources { get; set; } = new();
    }
}
