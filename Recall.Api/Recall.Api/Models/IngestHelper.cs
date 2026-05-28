namespace Recall.Api.Models
{
    public class IngestHelper
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string SourceType { get; set; }
        public string? Url { get; set; } = string.Empty;
        public List<string> Tags { get; set; }
    }
}
