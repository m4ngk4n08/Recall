namespace Recall.Api.DTOs;

public class ItemResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public DateTime SavedAt { get; set; }
    public List<string> Tags { get; set; } = new();
}