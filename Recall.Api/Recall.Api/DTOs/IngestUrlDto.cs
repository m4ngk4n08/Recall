namespace Recall.Api.DTOs
{
    public class IngestUrlDto
    {
        public string Url { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
    }
}
