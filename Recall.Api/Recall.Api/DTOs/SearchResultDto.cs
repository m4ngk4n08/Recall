namespace Recall.Api.DTOs
{
    public class SearchResultDto : ItemResponseDto
    {
        public double Distance { get; set; } // cosine distance (0 = identical, 2 = opposite)
    }
}
