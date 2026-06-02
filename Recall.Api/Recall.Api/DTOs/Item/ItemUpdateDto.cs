namespace Recall.Api.DTOs.Item
{
    public class ItemUpdateDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string SourceType { get; set; } = "note";
        public string? SourceUrl { get; set; }
        public List<string> Tags { get; set; }
    }
}
