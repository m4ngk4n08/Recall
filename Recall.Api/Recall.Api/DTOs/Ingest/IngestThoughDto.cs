namespace Recall.Api.DTOs.Ingest
{
    public class IngestThoughDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
    }
}
