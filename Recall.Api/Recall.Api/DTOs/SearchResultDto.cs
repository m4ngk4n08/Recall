namespace Recall.Api.DTOs
{
    public class SearchResultDto : ItemResponseDto
    {
        public Guid? ParentId { get; set; }
        public int ChunkIndex { get; set; }
        public double Distance { get; set; }   // Cosine distance from query
    }
}
